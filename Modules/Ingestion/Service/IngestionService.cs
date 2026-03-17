using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Alerts.Repository;
using Modules.Ingestion.Dto;
using Modules.Ingestion.Repository;
using Modules.Sensors.Repository;
using Shared.Entity;
using Shared.Enums;
using Shared.Notifications;

namespace Modules.Ingestion.Service;

/// <summary>
/// Service implementation for ingestion operations
/// </summary>
public class IngestionService(
    IIngestionRunRepository ingestionRunRepository,
    ISensorRepository sensorRepository,
    IAlertRepository alertRepository,
    IAlertTriggeredNotifier alertTriggeredNotifier,
    ILogger<IngestionService> logger) : IIngestionService
{
    /// <summary>
    /// Maximum number of readings per batch (API abuse prevention).
    /// </summary>
    private const int MaxBatchSize = 5000;

    /// <inheritdoc />
    [Span]
    public virtual async Task<IngestionRun> IngestReadingsAsync(
        Guid organizationId,
        IReadOnlyList<IngestReadingItem> readings,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Ingesting {Count} readings for organization {OrgId}", readings.Count, organizationId);

        if (readings.Count == 0)
            throw new InvalidOperationException("At least one reading is required");

        if (readings.Count > MaxBatchSize)
            throw new InvalidOperationException($"Batch size cannot exceed {MaxBatchSize}.");

        var run = new IngestionRun
        {
            Id = Guid.NewGuid(),
            Source = IngestionSource.Api,
            Status = IngestionStatus.InProgress,
            TotalRecords = readings.Count,
            AcceptedRecords = 0,
            RejectedRecords = 0,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = null,
            OrganizationId = organizationId
        };

        var acceptedReadings = new List<SensorReading>();
        var rejectedReadings = new List<IngestionRejectedReading>();

        // Track sensor context for accepted readings to use during alert evaluation
        var sensorByReadingIndex = new Dictionary<int, Sensor>();

        for (var i = 0; i < readings.Count; i++)
        {
            var item = readings[i];
            var (accepted, sensor, rejectionReason) = await ValidateAndBuildReadingAsync(organizationId, item, cancellationToken);
            if (accepted != null && sensor != null)
            {
                acceptedReadings.Add(accepted);
                sensorByReadingIndex[acceptedReadings.Count - 1] = sensor;
            }
            else
            {
                rejectedReadings.Add(new IngestionRejectedReading
                {
                    Id = Guid.NewGuid(),
                    IngestionRunId = run.Id,
                    RowIndex = i,
                    SensorId = item.SensorId,
                    RejectionReason = rejectionReason!
                });
            }
        }

        run.AcceptedRecords = acceptedReadings.Count;
        run.RejectedRecords = rejectedReadings.Count;
        run.CompletedAt = DateTimeOffset.UtcNow;
        run.Status = run.RejectedRecords == 0
            ? IngestionStatus.Success
            : run.AcceptedRecords == 0
                ? IngestionStatus.Failed
                : IngestionStatus.PartialSuccess;

        var (newAlerts, sensorBreachStateUpdates, autoResolvedAlerts) =
            await BuildNewAlertsAsync(organizationId, acceptedReadings, sensorByReadingIndex, cancellationToken);

        await ingestionRunRepository.CreateRunAsync(
            run,
            rejectedReadings,
            acceptedReadings,
            newAlerts,
            sensorBreachStateUpdates,
            cancellationToken);

        // Notify recipients for each newly triggered alert (email, etc.). Do not fail ingestion if notification fails.
        if (newAlerts.Count > 0)
            logger.LogInformation("Notifying for {Count} new alert(s)", newAlerts.Count);
        foreach (var alert in newAlerts)
        {
            try
            {
                await alertTriggeredNotifier.NotifyTriggeredAsync(alert, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Alert notification failed for alert {AlertId}; ingestion completed successfully", alert.Id);
            }
        }

        // Automatically resolve alerts whose sensor values returned to normal during this batch
        foreach (var (alertId, resolvedAt) in autoResolvedAlerts)
        {
            var alert = await alertRepository.GetByIdAsync(alertId);
            if (alert == null)
                continue;

            if (alert.Status != AlertStatus.Active && alert.Status != AlertStatus.Acknowledged)
                continue;

            alert.Status = AlertStatus.Resolved;
            alert.ResolvedAt = resolvedAt;
            await alertRepository.UpdateAsync(alert);

            await alertRepository.AddEventAsync(new AlertEvent
            {
                AlertId = alert.Id,
                OrganizationId = alert.OrganizationId,
                EventType = AlertEventType.Resolved,
                OccurredAt = resolvedAt,
                Description = "Alert auto-resolved (value returned to normal)",
            });
        }

        logger.LogInformation("Ingestion run {RunId} completed: {Accepted} accepted, {Rejected} rejected, {Alerts} alerts",
            run.Id, run.AcceptedRecords, run.RejectedRecords, newAlerts.Count);

        return run;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<IngestionRun> Items, int TotalCount)> GetRunsAsync(
        Guid organizationId,
        GetIngestionRunsRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting ingestion runs for organization {OrgId}", organizationId);
        return await ingestionRunRepository.GetPagedAsync(organizationId, request, cancellationToken);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<IngestionRun> GetRunByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting ingestion run {Id} for organization {OrgId}", id, organizationId);

        var run = await ingestionRunRepository.GetByIdAsync(id, cancellationToken);

        if (run == null || run.OrganizationId != organizationId)
        {
            logger.LogWarning("Ingestion run not found: {Id}", id);
            throw new KeyNotFoundException($"Ingestion run with ID {id} was not found");
        }

        return run;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<IngestionStatsResponse> GetStatsAsync(
        Guid organizationId,
        GetIngestionRunsRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting ingestion stats for organization {OrgId}", organizationId);
        return await ingestionRunRepository.GetStatsAsync(organizationId, request, cancellationToken);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<SensorRejectionCountResponse> GetSensorRejectionCountAsync(
        Guid organizationId,
        Guid sensorId,
        GetSensorRejectionCountRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting rejection count for sensor {SensorId} in organization {OrgId}", sensorId, organizationId);

        var from = request.From ?? DateTime.UtcNow.AddHours(-24);
        var to = request.To ?? DateTime.UtcNow;

        var count = await ingestionRunRepository.GetRejectedCountBySensorAsync(
            organizationId, sensorId, from, to, cancellationToken);

        return new SensorRejectionCountResponse
        {
            SensorId = sensorId,
            Count = count
        };
    }

    private async Task<(SensorReading? Accepted, Sensor? Sensor, string? RejectionReason)> ValidateAndBuildReadingAsync(
        Guid organizationId,
        IngestReadingItem item,
        CancellationToken cancellationToken)
    {
        var sensor = await sensorRepository.GetByIdAsync(item.SensorId);
        if (sensor == null)
            return (null, null, "Sensor not found or access denied");
        if (sensor.OrganizationId != organizationId)
            return (null, null, "Sensor not found or access denied");

        if (!Enum.IsDefined(typeof(Unit), item.Unit))
            return (null, null, "Invalid unit");

        var reading = new SensorReading
        {
            SensorId = item.SensorId,
            Value = item.Value,
            TimestampUtc = item.TimestampUtc,
            Unit = item.Unit,
            OrganizationId = organizationId
        };
        return (reading, sensor, null);
    }

    /// <summary>
    /// Derives alert severity from the breach: above max = Critical; below min = Warning or Info
    /// (Info when only slightly below min, so simulators can trigger all severity levels).
    /// </summary>
    private static AlertSeverity DeriveSeverity(decimal value, decimal thresholdMin, decimal thresholdMax)
    {
        if (value > thresholdMax)
            return AlertSeverity.Critical;
        var rangeSpan = thresholdMax - thresholdMin;
        var margin = thresholdMin - value; // positive when value is below min
        var infoBand = Math.Max(rangeSpan * 0.1m, 0.001m);
        return margin <= infoBand ? AlertSeverity.Info : AlertSeverity.Warning;
    }

    /// <summary>
    /// Builds new alerts respecting threshold duration: only trigger after value stays out of range
    /// for the configured duration; reset the timer when value returns to normal. Also determines
    /// which existing alerts should be automatically resolved when values return to the allowed range.
    /// </summary>
    private async Task<(List<Alert> newAlerts, IReadOnlyDictionary<Guid, DateTime?> sensorBreachStateUpdates, IReadOnlyList<(Guid AlertId, DateTime ResolvedAt)> autoResolvedAlerts)> BuildNewAlertsAsync(
        Guid organizationId,
        List<SensorReading> acceptedReadings,
        Dictionary<int, Sensor> sensorByReadingIndex,
        CancellationToken cancellationToken)
    {
        var newAlerts = new List<Alert>();
        var breachStateUpdates = new Dictionary<Guid, DateTime?>();
        var autoResolvedAlerts = new List<(Guid AlertId, DateTime ResolvedAt)>();

        // Group (reading, sensor) by sensor Id and process in timestamp order per sensor
        var bySensor = new Dictionary<Guid, List<(SensorReading Reading, Sensor Sensor)>>();
        for (var i = 0; i < acceptedReadings.Count; i++)
        {
            if (!sensorByReadingIndex.TryGetValue(i, out var sensor))
                continue;
            var reading = acceptedReadings[i];
            if (!bySensor.TryGetValue(sensor.Id, out var list))
            {
                list = new List<(SensorReading, Sensor)>();
                bySensor[sensor.Id] = list;
            }
            list.Add((reading, sensor));
        }

        var alertedSensorIds = new HashSet<Guid>();

        foreach (var (sensorId, readingSensorList) in bySensor)
        {
            var sensor = readingSensorList[0].Sensor;
            var threshold = sensor.Threshold;
            if (threshold == null)
                continue;

            var duration = threshold.Duration;
            var readingsInOrder = readingSensorList
                .Select(x => x.Reading)
                .OrderBy(r => r.TimestampUtc)
                .ToList();

            DateTime? firstOutOfRangeAt = sensor.FirstOutOfRangeAt;

            var existingUnresolved = await alertRepository.GetUnresolvedBySensorIdAsync(sensorId);
            if (existingUnresolved != null)
            {
                // Existing alert: update breach state and detect when the value returns to normal.
                // If any in-range reading is observed, we mark the alert for auto-resolution.
                DateTime? autoResolveAt = null;

                foreach (var reading in readingsInOrder)
                {
                    var isOutOfRange = reading.Value < threshold.Min || reading.Value > threshold.Max;
                    if (isOutOfRange)
                    {
                        if (firstOutOfRangeAt == null)
                            firstOutOfRangeAt = reading.TimestampUtc;
                    }
                    else
                    {
                        if (autoResolveAt == null)
                            autoResolveAt = reading.TimestampUtc;

                        firstOutOfRangeAt = null;
                    }
                }

                breachStateUpdates[sensorId] = firstOutOfRangeAt;

                if (autoResolveAt.HasValue)
                    autoResolvedAlerts.Add((existingUnresolved.Id, autoResolveAt.Value));

                continue;
            }

            foreach (var reading in readingsInOrder)
            {
                var isOutOfRange = reading.Value < threshold.Min || reading.Value > threshold.Max;
                if (isOutOfRange)
                {
                    if (firstOutOfRangeAt == null)
                        firstOutOfRangeAt = reading.TimestampUtc;

                    var durationMet = duration <= TimeSpan.Zero
                        || (reading.TimestampUtc - firstOutOfRangeAt.Value) >= duration;

                    if (durationMet && !alertedSensorIds.Contains(sensorId))
                    {
                        var severity = DeriveSeverity(reading.Value, threshold.Min, threshold.Max);
                        newAlerts.Add(new Alert
                        {
                            Id = Guid.NewGuid(),
                            SensorId = sensor.Id,
                            EquipmentId = sensor.EquipmentId,
                            SiteId = sensor.Equipment.SiteId,
                            OrganizationId = organizationId,
                            Severity = severity,
                            TriggeredValue = reading.Value,
                            ThresholdMin = threshold.Min,
                            ThresholdMax = threshold.Max,
                            TriggeredAt = reading.TimestampUtc,
                            Status = AlertStatus.Active,
                            ResolvedAt = null,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = null,
                            DeletedAt = null
                        });
                        alertedSensorIds.Add(sensorId);
                        firstOutOfRangeAt = null;
                    }
                }
                else
                {
                    firstOutOfRangeAt = null;
                }
            }

            breachStateUpdates[sensorId] = firstOutOfRangeAt;
        }

        return (newAlerts, breachStateUpdates, autoResolvedAlerts);
    }
}
