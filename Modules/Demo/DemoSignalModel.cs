using Shared.Entity;
using Shared.Enums;

namespace Modules.Demo;

/// <summary>Sensor snapshot used by the demo generator (thresholds are read from the DB, never assumed).</summary>
internal sealed record DemoSensor(
    Guid Id,
    string Serial,
    SensorTypeKind Kind,
    Unit Unit,
    decimal Min,
    decimal Max,
    Guid EquipmentId,
    Guid SiteId,
    string EquipmentName)
{
    public static DemoSensor From(Sensor s) => new(
        s.Id,
        s.Serial,
        s.SensorType?.Type ?? SensorTypeKind.Custom,
        s.SensorType?.Unit ?? Shared.Enums.Unit.Custom,
        s.Threshold?.Min ?? 0m,
        s.Threshold?.Max ?? 100m,
        s.EquipmentId,
        s.Equipment?.SiteId ?? Guid.Empty,
        s.Equipment?.Name ?? string.Empty);
}

/// <summary>
/// An out-of-range period for one sensor. Values are pushed outside the threshold between Start and End.
/// Direction +1 = above max (Critical), -1 = below min (Warning / Info depending on the margin).
/// </summary>
internal sealed record DemoEpisode(
    string Serial,
    DateTime Start,
    DateTime End,
    int Direction,
    double Magnitude,
    TimeSpan? AckAfter,
    bool ManualResolve,
    bool Persistent)
{
    /// <summary>
    /// When the alert fires. The real pipeline fires on the first out-of-range reading after the threshold
    /// duration (30 s) — with a 5-min grid that is the second out-of-range reading.
    /// </summary>
    public DateTime TriggerAt => Start.AddMinutes(5);

    public bool Contains(DateTime t) => t >= Start && t < End;
}

/// <summary>
/// Deterministic signal model for demo sensors: type-appropriate baselines with daily cycles + noise,
/// a fixed set of "scenario" sensors, and a deterministic schedule of out-of-range episodes.
/// The same (sensor, timestamp) always yields the same value, so startup backfill, top-ups and the live
/// loop all agree with each other and with the alerts that are seeded.
/// </summary>
internal sealed class DemoSignalModel
{
    /// <summary>Sensor whose latest reading always lags ~11 min (Sensor Health: "Stale").</summary>
    public const string StaleSerial = "Supply Pressure";

    /// <summary>How far behind "now" the stale sensor reports (Stale = age &gt; 600 s and ≤ 1500 s).</summary>
    public static readonly TimeSpan StaleLag = TimeSpan.FromMinutes(11);

    /// <summary>Sensor that drops 1 of every 6 readings (~83 % reliability → Sensor Health: "Warning"/flapping).</summary>
    public const string GapSerial = "Generator Energy";

    /// <summary>Env var: set to false so the demo never produces Critical alerts (no always-on critical scenario and
    /// every out-of-range episode stays below the min → Warning / Info only; dashboard shows "Degraded").</summary>
    public const string ActiveCriticalEnvVar = "DEMO_ACTIVE_CRITICAL";

    private sealed record PersistentSpec(
        string Serial, int AnchorHourUtc, int GapMinutes, int Direction, double Magnitude, int? AckAfterMinutes, bool Critical);

    /// <summary>
    /// Always-on scenarios. Each runs in 24 h cycles: in range for GapMinutes at the anchor hour (the open alert
    /// auto-resolves), then out of range until the next anchor (a new alert fires). So there is always an open
    /// alert, never older than a day, plus one resolved occurrence per day in the history.
    /// </summary>
    private static readonly PersistentSpec[] PersistentSpecs =
    [
        // Cold storage above +5 °C → Critical, not acknowledged yet
        new("Cold Storage Temp", AnchorHourUtc: 3, GapMinutes: 15, Direction: +1, Magnitude: 0.22, AckAfterMinutes: null, Critical: true),
        // Humidity well below min → Warning, active
        new("Humidity Sensor 2", AnchorHourUtc: 9, GapMinutes: 20, Direction: -1, Magnitude: 0.18, AckAfterMinutes: null, Critical: false),
        // Chiller pressure low → Warning, acknowledged ("In Progress")
        new("Chiller Pressure", AnchorHourUtc: 14, GapMinutes: 20, Direction: -1, Magnitude: 0.15, AckAfterMinutes: 12, Critical: false),
        // O2 slightly low → Info, acknowledged
        new("O2 Monitor 2", AnchorHourUtc: 19, GapMinutes: 15, Direction: -1, Magnitude: 0.06, AckAfterMinutes: 8, Critical: false),
    ];

    private const int TransientEpisodesPerDay = 3;

    private readonly Dictionary<string, DemoSensor> _bySerial;
    private readonly PersistentSpec[] _persistent;
    private readonly string[] _transientPool;
    private readonly Dictionary<int, List<DemoEpisode>> _transientCache = new();
    private readonly bool _allowCritical;

    public DemoSignalModel(IEnumerable<DemoSensor> sensors)
    {
        _bySerial = sensors
            .GroupBy(s => s.Serial, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var includeCritical = !string.Equals(
            Environment.GetEnvironmentVariable(ActiveCriticalEnvVar)?.Trim(), "false", StringComparison.OrdinalIgnoreCase);

        _allowCritical = includeCritical;
        _persistent = PersistentSpecs
            .Where(p => _bySerial.ContainsKey(p.Serial) && (includeCritical || !p.Critical))
            .ToArray();

        var special = new HashSet<string>(PersistentSpecs.Select(p => p.Serial), StringComparer.OrdinalIgnoreCase)
        {
            StaleSerial,
            GapSerial,
        };
        _transientPool = _bySerial.Keys
            .Where(s => !special.Contains(s))
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToArray();
    }

    public IReadOnlyCollection<DemoSensor> Sensors => _bySerial.Values;

    /// <summary>False when DEMO_ACTIVE_CRITICAL=false: no episode ever goes above max (above max is always Critical).</summary>
    public bool AllowCritical => _allowCritical;

    public bool IsKnown(string serial) => _bySerial.ContainsKey(serial);

    public DemoSensor? Get(string serial) => _bySerial.GetValueOrDefault(serial);

    public static bool IsStale(DemoSensor s) => string.Equals(s.Serial, StaleSerial, StringComparison.OrdinalIgnoreCase);

    public static bool IsGap(DemoSensor s) => string.Equals(s.Serial, GapSerial, StringComparison.OrdinalIgnoreCase);

    /// <summary>True when the gap sensor intentionally skips this 5-min slot.</summary>
    public static bool IsDroppedSlot(DateTime t) => (t.Ticks / TimeSpan.FromMinutes(5).Ticks) % 6 == 3;

    // ---------------------------------------------------------------- episodes

    /// <summary>Episode covering timestamp t for the sensor, if any.</summary>
    public DemoEpisode? EpisodeAt(string serial, DateTime t)
    {
        foreach (var spec in _persistent)
        {
            if (!string.Equals(spec.Serial, serial, StringComparison.OrdinalIgnoreCase))
                continue;
            var ep = PersistentOccurrence(spec, t);
            if (ep.Contains(t))
                return ep;
        }

        var day = DateOnly.FromDateTime(t);
        foreach (var d in new[] { day.AddDays(-1), day })
        {
            foreach (var ep in TransientForDay(d))
            {
                if (string.Equals(ep.Serial, serial, StringComparison.OrdinalIgnoreCase) && ep.Contains(t))
                    return ep;
            }
        }
        return null;
    }

    /// <summary>All episodes (persistent occurrences + transient) that overlap [from, to], ordered by start.</summary>
    public List<DemoEpisode> EpisodesBetween(DateTime from, DateTime to)
    {
        var list = new List<DemoEpisode>();
        foreach (var spec in _persistent)
        {
            var ep = PersistentOccurrence(spec, from);
            while (ep.Start <= to)
            {
                if (ep.End > from)
                    list.Add(ep);
                ep = PersistentOccurrence(spec, ep.End.AddMinutes(spec.GapMinutes + 1));
            }
        }

        for (var d = DateOnly.FromDateTime(from).AddDays(-1); d <= DateOnly.FromDateTime(to); d = d.AddDays(1))
        {
            list.AddRange(TransientForDay(d).Where(e => e.End > from && e.Start <= to));
        }

        return list.OrderBy(e => e.Start).ToList();
    }

    private static DemoEpisode PersistentOccurrence(PersistentSpec spec, DateTime t)
    {
        // Cycle start = most recent anchor hour ≤ t
        var anchor = t.Date.AddHours(spec.AnchorHourUtc);
        if (anchor > t)
            anchor = anchor.AddDays(-1);
        var start = anchor.AddMinutes(spec.GapMinutes);
        var end = anchor.AddDays(1);
        return new DemoEpisode(
            spec.Serial,
            DateTime.SpecifyKind(start, DateTimeKind.Utc),
            DateTime.SpecifyKind(end, DateTimeKind.Utc),
            spec.Direction,
            spec.Magnitude,
            spec.AckAfterMinutes.HasValue ? TimeSpan.FromMinutes(spec.AckAfterMinutes.Value) : null,
            ManualResolve: false,
            Persistent: true);
    }

    private List<DemoEpisode> TransientForDay(DateOnly day)
    {
        if (_transientCache.TryGetValue(day.DayNumber, out var cached))
            return cached;

        var list = new List<DemoEpisode>();
        if (_transientPool.Length > 0)
        {
            var rng = new DemoRng(DemoHash.Combine(DemoHash.Of("transient-episodes"), day.DayNumber));
            var dayStart = day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            for (var i = 0; i < TransientEpisodesPerDay; i++)
            {
                var serial = _transientPool[rng.Next(0, _transientPool.Length)];
                var start = dayStart.AddMinutes(Math.Round(rng.Range(30, 23 * 60)));
                start = new DateTime(start.Ticks - start.Ticks % TimeSpan.FromMinutes(5).Ticks, DateTimeKind.Utc).AddMinutes(1);
                var end = start.AddMinutes(rng.Next(25, 85));
                // Draw the chance unconditionally so the rest of the deterministic schedule is unchanged.
                var goesAboveMax = rng.Chance(0.55);
                var direction = goesAboveMax && _allowCritical ? +1 : -1;
                var magnitude = rng.Range(0.06, 0.2);
                TimeSpan? ack = rng.Chance(0.5) ? TimeSpan.FromMinutes(rng.Next(6, 16)) : null;
                var manual = rng.Chance(0.4);

                if (list.Any(e => e.Serial == serial && e.Start < end.AddHours(1) && start < e.End.AddHours(1)))
                    continue;

                list.Add(new DemoEpisode(serial, start, end, direction, magnitude, ack, manual, Persistent: false));
            }
        }

        _transientCache[day.DayNumber] = list;
        return list;
    }

    // ---------------------------------------------------------------- values

    /// <summary>Reading value for a sensor at time t (rounded to 2 decimals).</summary>
    public decimal ValueAt(DemoSensor sensor, DateTime t)
    {
        var min = (double)sensor.Min;
        var max = (double)sensor.Max;
        var span = Math.Max(max - min, 0.01);
        var seed = DemoHash.Of(sensor.Serial);
        var minuteIndex = t.Ticks / TimeSpan.TicksPerMinute;
        var noise = DemoHash.Signed(DemoHash.Combine(seed, minuteIndex));

        var episode = EpisodeAt(sensor.Serial, t);
        double value;
        if (episode != null)
        {
            var ramp = Math.Clamp((t - episode.Start).TotalMinutes / 20.0, 0, 1);
            var excess = span * episode.Magnitude * (0.85 + 0.15 * ramp) + Math.Abs(noise) * span * 0.015 + 0.01;
            value = episode.Direction > 0 ? max + excess : min - excess;
        }
        else
        {
            value = Baseline(sensor.Kind, min, max, span, seed, t, noise);
        }

        return Math.Round((decimal)value, 2, MidpointRounding.AwayFromZero);
    }

    private static double Baseline(SensorTypeKind kind, double min, double max, double span, ulong seed, DateTime t, double noise)
    {
        // Sites are in North America; use UTC-6 as "site local" time for daily cycles.
        var local = t.AddHours(-6);
        var hour = local.Hour + local.Minute / 60.0;
        var daily = Math.Sin(2 * Math.PI * (hour - 9) / 24.0);                 // peaks ~15:00 local
        var business = hour is > 7 and < 19 ? Math.Sin(Math.PI * (hour - 7) / 12.0) : 0; // occupancy bump
        var phase = DemoHash.Unit(seed) * 2 * Math.PI;
        var wander = Math.Sin(t.Ticks / (double)TimeSpan.TicksPerHour / 7.3 + phase);  // slow drift
        var mid = (min + max) / 2;

        double v = kind switch
        {
            SensorTypeKind.Temperature => TemperatureCenter(min, max) + Math.Min(0.12 * span, 2.5) * daily
                                          + 0.04 * span * wander + 0.02 * span * noise,
            SensorTypeKind.Humidity => mid - 0.12 * span * daily + 0.05 * span * wander + 0.02 * span * noise,
            SensorTypeKind.Pressure => mid + 0.05 * span * daily + 0.03 * span * wander + 0.015 * span * noise,
            SensorTypeKind.Energy => min + span * (0.25 + 0.30 * business) + 0.04 * span * wander + 0.02 * span * noise,
            SensorTypeKind.Co2 => min + span * (0.15 + 0.35 * business) + 0.03 * span * wander + 0.015 * span * noise,
            SensorTypeKind.O2 => mid + 0.04 * span * wander + 0.02 * span * noise,
            _ => mid + 0.1 * span * daily + 0.02 * span * noise,
        };

        // Normal operation always stays comfortably inside the threshold band.
        return Math.Clamp(v, min + 0.08 * span, max - 0.08 * span);
    }

    private static double TemperatureCenter(double min, double max)
    {
        // Room-temperature sensors hover around 22 °C when their band allows it; cold chain uses the band middle.
        const double room = 22;
        return max - min >= 12 && room > min + 3 && room < max - 3 ? room : (min + max) / 2;
    }

    /// <summary>Same rule as the ingestion pipeline: above max = Critical; below min = Warning, or Info within 10 % of the band.</summary>
    public static AlertSeverity DeriveSeverity(decimal value, decimal thresholdMin, decimal thresholdMax)
    {
        if (value > thresholdMax)
            return AlertSeverity.Critical;
        var rangeSpan = thresholdMax - thresholdMin;
        var margin = thresholdMin - value;
        var infoBand = Math.Max(rangeSpan * 0.1m, 0.001m);
        return margin <= infoBand ? AlertSeverity.Info : AlertSeverity.Warning;
    }

    public static bool IsOutOfRange(DemoSensor s, decimal value) => value < s.Min || value > s.Max;
}
