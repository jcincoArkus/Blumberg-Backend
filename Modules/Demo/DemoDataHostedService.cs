using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modules.Alerts.Repository;
using Modules.Alerts.Service;
using Modules.Ingestion.Dto;
using Modules.Ingestion.Repository;
using Modules.Ingestion.Service;
using Modules.Sensors.Repository;
using Shared.Entity;
using Shared.Enums;

namespace Modules.Demo;

/// <summary>
/// DEMO_MODE only. Runs entirely in the background so the API binds its port immediately:
/// 1) startup backfill / top-up of sensor readings, ingestion runs, alerts and inventory activity (idempotent);
/// 2) a live loop that pushes one reading per sensor every ~60 s through the real ingestion pipeline
///    (so breach detection, alert creation and auto-resolve are the production code paths), acknowledges
///    scenario alerts, advances inventory activity and prunes old data.
/// Every step is wrapped so a failure is logged and never crashes the API.
/// </summary>
internal sealed class DemoDataHostedService(ILoggerFactory loggerFactory) : BackgroundService
{
    private readonly ILogger _logger = loggerFactory.CreateLogger("DemoData");

    private const string OrganizationSlug = "blumberg";

    private DateTime _lastStaleWrite = DateTime.MinValue;
    private long _lastGapSlot = -1;
    private long _tick;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Yield immediately so host startup (Kestrel binding the port) is never delayed.
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

            var orgId = await ResolveOrganizationAsync(stoppingToken);
            if (orgId == null)
                return;

            _logger.LogInformation("Demo mode: starting background backfill for organization {OrgId}", orgId);
            var started = DateTime.UtcNow;

            await SafeAsync("sensor backfill", () => new DemoSensorDataSeeder(_logger).RunAsync(orgId.Value, stoppingToken));
            await SafeAsync("inventory backfill", () => new DemoInventorySeeder(_logger).RunAsync(DateTime.UtcNow, stoppingToken));

            _logger.LogInformation("Demo mode: backfill finished in {Seconds:0.0}s; live loop every {Interval}s",
                (DateTime.UtcNow - started).TotalSeconds, DemoMode.LiveInterval.TotalSeconds);

            using var timer = new PeriodicTimer(DemoMode.LiveInterval);
            do
            {
                _tick++;
                await SafeAsync("live tick", () => LiveTickAsync(orgId.Value, stoppingToken));

                if (_tick % 10 == 0)
                    await SafeAsync("inventory tick", () => new DemoInventorySeeder(_logger).RunAsync(DateTime.UtcNow, stoppingToken));
                if (_tick % 60 == 0)
                    await SafeAsync("prune", () => DemoSensorDataSeeder.PruneAsync(orgId.Value, DateTime.UtcNow, _logger, stoppingToken));
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // shutting down
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Demo mode: background service stopped unexpectedly (API keeps running)");
        }
    }

    private async Task SafeAsync(string step, Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Demo mode: {Step} failed", step);
        }
    }

    private async Task<Guid?> ResolveOrganizationAsync(CancellationToken ct)
    {
        for (var attempt = 1; attempt <= 12; attempt++)
        {
            try
            {
                await using var ctx = DemoDb.Create(null);
                var org = await ctx.Organizations.AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Slug == OrganizationSlug && o.DeletedAt == null, ct);
                if (org != null)
                    return org.Id;
                _logger.LogWarning("Demo mode: organization '{Slug}' not found (attempt {Attempt})", OrganizationSlug, attempt);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Demo mode: database not ready (attempt {Attempt})", attempt);
            }
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }

        _logger.LogError("Demo mode: giving up — organization '{Slug}' not available", OrganizationSlug);
        return null;
    }

    /// <summary>One live tick: a reading per sensor via the real ingestion pipeline, plus scenario acknowledgements.</summary>
    private async Task LiveTickAsync(Guid orgId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        await using var ctx = DemoDb.Create(orgId);
        var sensors = await DemoSensorDataSeeder.LoadSensorsAsync(ctx, ct);
        if (sensors.Count == 0)
            return;
        var model = new DemoSignalModel(sensors);

        var items = new List<IngestReadingItem>();
        foreach (var sensor in sensors)
        {
            var ts = now;
            if (DemoSignalModel.IsStale(sensor))
            {
                // Always ~11 min behind → "Stale" in Sensor Health
                ts = now - DemoSignalModel.StaleLag;
                if (ts <= _lastStaleWrite)
                    continue;
                _lastStaleWrite = ts;
            }
            else if (DemoSignalModel.IsGap(sensor))
            {
                // 5-min cadence with 1 of 6 slots dropped → reliability ~83 % ("Warning")
                var slot = now.Ticks / TimeSpan.FromMinutes(5).Ticks;
                if (slot == _lastGapSlot || DemoSignalModel.IsDroppedSlot(now))
                    continue;
                _lastGapSlot = slot;
            }

            items.Add(new IngestReadingItem
            {
                SensorId = sensor.Id,
                Value = model.ValueAt(sensor, ts),
                TimestampUtc = ts,
                Unit = sensor.Unit,
            });
        }

        // Every ~17 ticks one bad row from a misconfigured gateway → real rejection in the ingestion stats.
        if (_tick % 17 == 5)
        {
            items.Add(_tick % 2 == 0
                ? new IngestReadingItem { SensorId = Guid.NewGuid(), Value = 21.5m, TimestampUtc = now, Unit = Unit.Celsius }
                : new IngestReadingItem { SensorId = sensors[(int)(_tick % sensors.Count)].Id, Value = 0m, TimestampUtc = now, Unit = (Unit)99 });
        }

        if (items.Count == 0)
            return;

        var alertRepository = new AlertRepository(ctx, loggerFactory.CreateLogger<AlertRepository>());
        var ingestion = new IngestionService(
            new IngestionRunRepository(ctx, alertRepository, loggerFactory.CreateLogger<IngestionRunRepository>()),
            new SensorRepository(ctx, loggerFactory.CreateLogger<SensorRepository>()),
            alertRepository,
            new NoOpAlertTriggeredNotifier(),
            loggerFactory.CreateLogger<IngestionService>());

        await ingestion.IngestReadingsAsync(orgId, items, ct);

        await AcknowledgeScenarioAlertsAsync(ctx, alertRepository, model, now);
    }

    /// <summary>Scenario alerts with an "ack after" delay get acknowledged (plus a technician note), like an operator would.</summary>
    private async Task AcknowledgeScenarioAlertsAsync(
        Adapters.Database.ApplicationDbContext ctx, AlertRepository alertRepository, DemoSignalModel model, DateTime now)
    {
        var active = await ctx.Alerts.AsNoTracking()
            .Where(a => a.DeletedAt == null && a.Status == AlertStatus.Active)
            .Select(a => new { a.Id, a.SensorId, a.TriggeredAt, a.OrganizationId })
            .ToListAsync();
        if (active.Count == 0)
            return;

        var sensorsById = model.Sensors.ToDictionary(s => s.Id);
        var actorId = await ctx.Admins.AsNoTracking()
            .OrderBy(a => a.Email != "admin@blumberg.com").ThenBy(a => a.CreatedAt)
            .Select(a => (Guid?)a.Id)
            .FirstOrDefaultAsync();
        var alertService = new AlertService(alertRepository, loggerFactory.CreateLogger<AlertService>());

        foreach (var a in active)
        {
            if (!sensorsById.TryGetValue(a.SensorId, out var sensor))
                continue;
            var episode = model.EpisodeAt(sensor.Serial, a.TriggeredAt);
            if (episode?.AckAfter == null || now < a.TriggeredAt + episode.AckAfter.Value)
                continue;

            ctx.ChangeTracker.Clear();
            await alertService.AcknowledgeAsync(a.Id);
            await alertRepository.AddEventAsync(new AlertEvent
            {
                AlertId = a.Id,
                OrganizationId = a.OrganizationId,
                EventType = AlertEventType.Note,
                OccurredAt = now.AddSeconds(5),
                Description = DemoSensorDataSeeder.NoteFor(sensor),
                ActorId = actorId,
            });
            _logger.LogInformation("Demo mode: acknowledged alert {AlertId} on {Sensor}", a.Id, sensor.Serial);
        }
    }
}
