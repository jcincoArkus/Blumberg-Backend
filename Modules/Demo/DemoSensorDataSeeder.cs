using Adapters.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;
using Shared.Enums;

namespace Modules.Demo;

/// <summary>
/// Startup backfill for the monitoring side of the demo (idempotent, safe on every restart):
/// <list type="bullet">
/// <item>tops up sensor readings from each sensor's latest reading to "now" (5-min grid for the last 26 h so the
/// 24 h reliability score is &gt; 90 % → Healthy; 30-min grid for older history; max 7 days back);</item>
/// <item>groups the new readings into hourly API / CSV ingestion runs with a few rejected rows;</item>
/// <item>reconciles alerts with the deterministic episode schedule (creates missing ones with their event
/// history, resolves open ones whose episode ended while the instance was asleep);</item>
/// <item>updates Sensor.LastSeenAt / FirstOutOfRangeAt and prunes data older than the retention window.</item>
/// </list>
/// </summary>
internal sealed class DemoSensorDataSeeder(ILogger logger)
{
    private static readonly TimeSpan HistoryWindow = TimeSpan.FromDays(7);
    private static readonly TimeSpan FineWindow = TimeSpan.FromHours(26);
    private static readonly TimeSpan FineStep = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan CoarseStep = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan Retention = TimeSpan.FromDays(8);

    private const string AutoResolvedDescription = "Alert auto-resolved (value returned to normal)";

    private static readonly string[] ApiRejectionReasons =
    [
        "Sensor not found or access denied",
        "Invalid unit",
    ];

    private static readonly string[] CsvRejectionReasons =
    [
        "Invalid unit",
        "Sensor not found or access denied",
        "Malformed CSV row",
        "Missing timestamp",
    ];

    public async Task RunAsync(Guid organizationId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        List<DemoSensor> sensors;
        await using (var ctx = DemoDb.Create(organizationId))
        {
            sensors = await LoadSensorsAsync(ctx, ct);
        }

        if (sensors.Count == 0)
        {
            logger.LogWarning("Demo: no sensors found for organization {OrgId}; skipping sensor backfill", organizationId);
            return;
        }

        var model = new DemoSignalModel(sensors);

        // Readings first: they decide what the dashboard shows right after a cold start.
        var (readings, runs, rejected) = await TopUpReadingsAsync(organizationId, model, now, ct);
        logger.LogInformation("Demo: topped up {Readings} readings in {Runs} ingestion runs ({Rejected} rejected rows)", readings, runs, rejected);

        var (created, resolved) = await ReconcileAlertsAsync(organizationId, model, now, ct);
        logger.LogInformation("Demo: alerts reconciled ({Created} created, {Resolved} resolved)", created, resolved);

        await UpdateSensorStateAsync(organizationId, model, ct);
        await PruneAsync(organizationId, now, logger, ct);
    }

    internal static async Task<List<DemoSensor>> LoadSensorsAsync(ApplicationDbContext ctx, CancellationToken ct)
    {
        var sensors = await ctx.Sensors
            .AsNoTracking()
            .Include(s => s.SensorType)
            .Include(s => s.Threshold)
            .Include(s => s.Equipment)
            .Where(s => s.DeletedAt == null && s.Status != SensorStatus.Inactive)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);
        return sensors.Select(DemoSensor.From).ToList();
    }

    // ------------------------------------------------------------------ readings + ingestion runs

    private async Task<(int Readings, int Runs, int Rejected)> TopUpReadingsAsync(
        Guid organizationId, DemoSignalModel model, DateTime now, CancellationToken ct)
    {
        await using var ctx = DemoDb.Create(organizationId);
        ctx.ChangeTracker.AutoDetectChangesEnabled = false;

        var ids = model.Sensors.Select(s => s.Id).ToList();
        var latestBySensor = await ctx.SensorReadings
            .Where(r => ids.Contains(r.SensorId))
            .GroupBy(r => r.SensorId)
            .Select(g => new { SensorId = g.Key, Max = g.Max(r => r.TimestampUtc) })
            .ToDictionaryAsync(x => x.SensorId, x => x.Max, ct);

        var windowStart = now - HistoryWindow;
        var fineStart = now - FineWindow;

        // (hour bucket, source) → readings
        var buckets = new SortedDictionary<(DateTime Hour, IngestionSource Source), List<SensorReading>>();

        foreach (var sensor in model.Sensors)
        {
            var end = DemoSignalModel.IsStale(sensor) ? now - DemoSignalModel.StaleLag : now;
            var from = latestBySensor.TryGetValue(sensor.Id, out var last) && last > windowStart ? last : windowStart;

            for (var t = NextSlot(from, fineStart); t <= end; t = NextSlot(t, fineStart))
            {
                if (DemoSignalModel.IsGap(sensor) && t >= fineStart && DemoSignalModel.IsDroppedSlot(t))
                    continue;

                var hour = new DateTime(t.Year, t.Month, t.Day, t.Hour, 0, 0, DateTimeKind.Utc);
                var source = hour.Hour % 12 == 8 && sensor.Kind is SensorTypeKind.Pressure or SensorTypeKind.Energy
                    ? IngestionSource.Csv
                    : IngestionSource.Api;

                if (!buckets.TryGetValue((hour, source), out var list))
                {
                    list = [];
                    buckets[(hour, source)] = list;
                }

                list.Add(new SensorReading
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    SensorId = sensor.Id,
                    Value = model.ValueAt(sensor, t),
                    TimestampUtc = t,
                    Unit = sensor.Unit,
                    CreatedAt = t.AddSeconds(2),
                });
            }
        }

        int totalReadings = 0, totalRuns = 0, totalRejected = 0, pending = 0;
        var sensorIds = model.Sensors.Select(s => s.Id).ToArray();

        foreach (var ((hour, source), readings) in buckets)
        {
            var lastTs = readings.Max(r => r.TimestampUtc);
            var completedAt = new DateTimeOffset(Min(hour.AddHours(1), now).AddSeconds(-5), TimeSpan.Zero);
            if (completedAt.UtcDateTime < lastTs)
                completedAt = new DateTimeOffset(lastTs.AddSeconds(3), TimeSpan.Zero);

            var h = DemoHash.Combine(DemoHash.Of("run"), lastTs.Ticks, (long)source);
            var rng = new DemoRng(h);
            var rejectedRows = new List<IngestionRejectedReading>();
            if (rng.Chance(source == IngestionSource.Csv ? 0.4 : 0.3))
            {
                var count = rng.Next(1, 4);
                var reasons = source == IngestionSource.Csv ? CsvRejectionReasons : ApiRejectionReasons;
                for (var i = 0; i < count; i++)
                {
                    var reason = rng.Pick(reasons);
                    rejectedRows.Add(new IngestionRejectedReading
                    {
                        Id = Guid.NewGuid(),
                        RowIndex = rng.Next(0, readings.Count + count),
                        SensorId = reason.StartsWith("Sensor not found", StringComparison.Ordinal)
                            ? Guid.NewGuid()
                            : rng.Pick(sensorIds),
                        RejectionReason = reason,
                    });
                }
            }

            var run = NewRun(organizationId, source, readings.Count, rejectedRows, completedAt);
            ctx.IngestionRuns.Add(run);
            foreach (var r in readings)
                r.IngestionRunId = run.Id;
            ctx.SensorReadings.AddRange(readings);
            totalRuns++;
            totalReadings += readings.Count;
            totalRejected += rejectedRows.Count;
            pending += readings.Count;

            // Occasionally a CSV upload fails completely (wrong template) — shows up as "Failed" in Data Ingestion.
            if (source == IngestionSource.Csv && rng.Chance(0.2))
            {
                var failedAt = new DateTimeOffset(hour.AddMinutes(20), TimeSpan.Zero);
                if (failedAt.UtcDateTime <= now)
                {
                    var rows = rng.Next(12, 41);
                    var failedRows = Enumerable.Range(0, rows).Select(i => new IngestionRejectedReading
                    {
                        Id = Guid.NewGuid(),
                        RowIndex = i,
                        SensorId = i % 3 == 0 ? rng.Pick(sensorIds) : null,
                        RejectionReason = i % 3 == 0 ? "Invalid unit" : "Malformed CSV row",
                    }).ToList();
                    ctx.IngestionRuns.Add(NewRun(organizationId, IngestionSource.Csv, 0, failedRows, failedAt));
                    totalRuns++;
                    totalRejected += rows;
                }
            }

            if (pending >= 4000)
            {
                ctx.ChangeTracker.DetectChanges();
                await ctx.SaveChangesAsync(ct);
                ctx.ChangeTracker.Clear();
                pending = 0;
            }
        }

        ctx.ChangeTracker.DetectChanges();
        await ctx.SaveChangesAsync(ct);
        return (totalReadings, totalRuns, totalRejected);
    }

    private static IngestionRun NewRun(
        Guid organizationId, IngestionSource source, int accepted, List<IngestionRejectedReading> rejected, DateTimeOffset completedAt)
    {
        var runId = Guid.NewGuid();
        foreach (var r in rejected)
            r.IngestionRunId = runId;

        var startedAt = completedAt.AddMilliseconds(-(400 + (accepted + rejected.Count) * 6));
        return new IngestionRun
        {
            Id = runId,
            OrganizationId = organizationId,
            Source = source,
            TotalRecords = accepted + rejected.Count,
            AcceptedRecords = accepted,
            RejectedRecords = rejected.Count,
            Status = rejected.Count == 0
                ? IngestionStatus.Success
                : accepted == 0 ? IngestionStatus.Failed : IngestionStatus.PartialSuccess,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            CreatedAt = startedAt.UtcDateTime,
            RejectedReadings = rejected,
        };
    }

    /// <summary>Next grid timestamp strictly after t: 5-min grid inside the fine window, 30-min grid before it.</summary>
    private static DateTime NextSlot(DateTime t, DateTime fineStart)
    {
        var step = t >= fineStart ? FineStep : CoarseStep;
        var next = new DateTime(t.Ticks - t.Ticks % step.Ticks + step.Ticks, DateTimeKind.Utc);
        if (step == CoarseStep && next > fineStart)
        {
            // Switch to the fine grid at the boundary
            var fine = new DateTime(fineStart.Ticks - fineStart.Ticks % FineStep.Ticks + FineStep.Ticks, DateTimeKind.Utc);
            next = fine > t ? fine : next;
        }
        return next;
    }

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;

    // ------------------------------------------------------------------ alerts

    private async Task<(int Created, int Resolved)> ReconcileAlertsAsync(
        Guid organizationId, DemoSignalModel model, DateTime now, CancellationToken ct)
    {
        await using var ctx = DemoDb.Create(organizationId);

        var admins = await ctx.Admins.AsNoTracking().OrderBy(a => a.CreatedAt).Select(a => new { a.Id, a.Email }).ToListAsync(ct);
        var actorId = admins.FirstOrDefault(a => a.Email == "admin@blumberg.com")?.Id ?? admins.FirstOrDefault()?.Id;
        var recipientCount = Math.Max(admins.Count, 1);

        var windowStart = now - HistoryWindow;
        var existing = await ctx.Alerts
            .Where(a => a.DeletedAt == null
                && (a.TriggeredAt >= windowStart.AddDays(-2) || a.Status != AlertStatus.Resolved))
            .ToListAsync(ct);

        var sensorsById = model.Sensors.ToDictionary(s => s.Id);
        var resolved = 0;

        // 1) Close open alerts whose episode ended while the instance was asleep (the live pipeline never saw the
        //    in-range reading that would have auto-resolved them).
        foreach (var alert in existing.Where(a => a.Status != AlertStatus.Resolved))
        {
            if (!sensorsById.TryGetValue(alert.SensorId, out var sensor))
                continue;
            var episode = model.EpisodeAt(sensor.Serial, alert.TriggeredAt);
            if (episode != null && episode.End > now)
                continue;
            if (episode == null && model.EpisodeAt(sensor.Serial, now) != null)
                continue;

            var resolvedAt = episode?.End ?? now;
            if (resolvedAt < alert.TriggeredAt)
                resolvedAt = now;
            alert.Status = AlertStatus.Resolved;
            alert.ResolvedAt = resolvedAt;
            alert.UpdatedAt = now;
            ctx.AlertEvents.Add(NewEvent(alert, AlertEventType.Resolved, resolvedAt, AutoResolvedDescription, null));
            resolved++;
        }

        // 2) Create alerts for scheduled episodes that have no alert yet.
        var created = 0;
        foreach (var episode in model.EpisodesBetween(windowStart, now))
        {
            if (episode.TriggerAt > now || episode.TriggerAt >= episode.End)
                continue;
            var sensor = model.Get(episode.Serial);
            if (sensor == null)
                continue;

            var ongoing = episode.End > now;
            var windowEnd = ongoing ? now : episode.End;
            var alreadyCovered = existing.Any(a => a.SensorId == sensor.Id
                && a.TriggeredAt >= episode.Start.AddMinutes(-1)
                && a.TriggeredAt <= windowEnd.AddMinutes(1));
            if (alreadyCovered)
                continue;
            if (ongoing && existing.Any(a => a.SensorId == sensor.Id && a.Status != AlertStatus.Resolved))
                continue;

            var triggeredValue = model.ValueAt(sensor, episode.TriggerAt);
            var severity = DemoSignalModel.DeriveSeverity(triggeredValue, sensor.Min, sensor.Max);
            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                SensorId = sensor.Id,
                EquipmentId = sensor.EquipmentId,
                SiteId = sensor.SiteId,
                Severity = severity,
                TriggeredValue = triggeredValue,
                ThresholdMin = sensor.Min,
                ThresholdMax = sensor.Max,
                TriggeredAt = episode.TriggerAt,
                Status = AlertStatus.Active,
                CreatedAt = episode.TriggerAt,
            };

            var events = new List<AlertEvent>
            {
                NewEvent(alert, AlertEventType.Triggered, episode.TriggerAt, "Alert triggered", null),
            };
            if (severity != AlertSeverity.Info)
            {
                events.Add(NewEvent(alert, AlertEventType.NotificationSent, episode.TriggerAt.AddSeconds(4),
                    $"Email notification sent to {recipientCount} recipient(s)", null));
            }

            var ackAt = episode.AckAfter.HasValue ? episode.TriggerAt + episode.AckAfter.Value : (DateTime?)null;
            if (ackAt.HasValue && ackAt.Value <= windowEnd)
            {
                alert.Status = AlertStatus.Acknowledged;
                events.Add(NewEvent(alert, AlertEventType.Acknowledged, ackAt.Value, "Alert acknowledged", actorId));
                var noteAt = ackAt.Value.AddMinutes(3);
                if (noteAt <= windowEnd)
                    events.Add(NewEvent(alert, AlertEventType.Note, noteAt, NoteFor(sensor), actorId));
            }

            if (!ongoing)
            {
                alert.Status = AlertStatus.Resolved;
                alert.ResolvedAt = episode.End;
                events.Add(episode.ManualResolve
                    ? NewEvent(alert, AlertEventType.Resolved, episode.End, "Alert resolved", actorId)
                    : NewEvent(alert, AlertEventType.Resolved, episode.End, AutoResolvedDescription, null));
            }

            alert.UpdatedAt = events.Max(e => e.OccurredAt);
            ctx.Alerts.Add(alert);
            ctx.AlertEvents.AddRange(events);
            existing.Add(alert);
            created++;
        }

        await ctx.SaveChangesAsync(ct);
        return (created, resolved);
    }

    internal static AlertEvent NewEvent(Alert alert, AlertEventType type, DateTime at, string description, Guid? actorId) => new()
    {
        Id = Guid.NewGuid(),
        AlertId = alert.Id,
        OrganizationId = alert.OrganizationId,
        EventType = type,
        OccurredAt = at,
        Description = description,
        ActorId = actorId,
        CreatedAt = at,
    };

    internal static string NoteFor(DemoSensor sensor) => sensor.Kind switch
    {
        SensorTypeKind.Temperature => $"Technician dispatched to {sensor.EquipmentName}; checking compressor and door seals.",
        SensorTypeKind.Pressure => $"Maintenance ticket opened for {sensor.EquipmentName}; inspecting refrigerant line pressure.",
        SensorTypeKind.Humidity => $"Dehumidifier cycle adjusted on {sensor.EquipmentName}; monitoring recovery.",
        SensorTypeKind.O2 => $"Ventilation check requested for {sensor.EquipmentName}; area cleared as a precaution.",
        SensorTypeKind.Co2 => $"Ventilation increased around {sensor.EquipmentName}; monitoring CO2 levels.",
        SensorTypeKind.Energy => $"Load review scheduled for {sensor.EquipmentName}.",
        _ => $"Investigating {sensor.EquipmentName}.",
    };

    // ------------------------------------------------------------------ sensor state + retention

    internal static async Task UpdateSensorStateAsync(Guid organizationId, DemoSignalModel model, CancellationToken ct)
    {
        await using var ctx = DemoDb.Create(organizationId);
        var ids = model.Sensors.Select(s => s.Id).ToList();

        var latest = await ctx.SensorReadings
            .Where(r => ids.Contains(r.SensorId))
            .GroupBy(r => r.SensorId)
            .Select(g => g.OrderByDescending(r => r.TimestampUtc).Select(r => new { r.SensorId, r.TimestampUtc, r.Value }).First())
            .ToListAsync(ct);
        var openAlertSensors = await ctx.Alerts
            .Where(a => a.DeletedAt == null && a.Status != AlertStatus.Resolved)
            .Select(a => a.SensorId)
            .Distinct()
            .ToListAsync(ct);

        var tracked = await ctx.Sensors.Where(s => ids.Contains(s.Id)).ToListAsync(ct);
        foreach (var sensor in tracked)
        {
            var l = latest.FirstOrDefault(x => x.SensorId == sensor.Id);
            if (l == null)
                continue;
            var demo = model.Sensors.First(s => s.Id == sensor.Id);
            sensor.LastSeenAt = l.TimestampUtc;
            sensor.FirstOutOfRangeAt = DemoSignalModel.IsOutOfRange(demo, l.Value) && !openAlertSensors.Contains(sensor.Id)
                ? l.TimestampUtc
                : null;
        }

        await ctx.SaveChangesAsync(ct);
    }

    internal static async Task PruneAsync(Guid organizationId, DateTime now, ILogger logger, CancellationToken ct)
    {
        await using var ctx = DemoDb.Create(organizationId);
        var cutoff = now - Retention;

        var readings = await ctx.SensorReadings
            .Where(r => r.OrganizationId == organizationId && r.TimestampUtc < cutoff)
            .ExecuteDeleteAsync(ct);
        var runs = await ctx.IngestionRuns
            .Where(r => r.OrganizationId == organizationId && r.CreatedAt < cutoff)
            .ExecuteDeleteAsync(ct);

        if (readings > 0 || runs > 0)
            logger.LogInformation("Demo: pruned {Readings} readings and {Runs} ingestion runs older than {Days} days", readings, runs, Retention.TotalDays);
    }
}
