using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Alerts.Repository;
using Modules.Ingestion.Dto;
using Modules.Ingestion.Repository;
using Modules.Sensors.Repository;
using Shared.Entity;
using Shared.Enums;

namespace Modules.Ingestion.Service;

/// <summary>
/// Service implementation for ingestion operations
/// </summary>
public class IngestionService(
    IIngestionRunRepository ingestionRunRepository,
    ISensorRepository sensorRepository,
    IAlertRepository alertRepository,
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

        var newAlerts = await BuildNewAlertsAsync(organizationId, acceptedReadings, sensorByReadingIndex, cancellationToken);

        await ingestionRunRepository.CreateRunAsync(run, rejectedReadings, acceptedReadings, newAlerts, cancellationToken);

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

    private async Task<List<Alert>> BuildNewAlertsAsync(
        Guid organizationId,
        List<SensorReading> acceptedReadings,
        Dictionary<int, Sensor> sensorByReadingIndex,
        CancellationToken cancellationToken)
    {
        var newAlerts = new List<Alert>();
        // Track sensor IDs that already have a new alert in this batch to avoid duplicates
        var alertedSensorIds = new HashSet<Guid>();

        for (var i = 0; i < acceptedReadings.Count; i++)
        {
            var reading = acceptedReadings[i];
            if (!sensorByReadingIndex.TryGetValue(i, out var sensor))
                continue;

            var threshold = sensor.Threshold;
            if (threshold == null)
                continue;

            var isOutOfRange = reading.Value < threshold.Min || reading.Value > threshold.Max;
            if (!isOutOfRange)
                continue;

            if (alertedSensorIds.Contains(sensor.Id))
                continue;

            var existingUnresolved = await alertRepository.GetUnresolvedBySensorIdAsync(sensor.Id);
            if (existingUnresolved != null)
                continue;

            var severity = reading.Value > threshold.Max ? AlertSeverity.Critical : AlertSeverity.Warning;

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

            alertedSensorIds.Add(sensor.Id);
        }

        return newAlerts;
    }
}
