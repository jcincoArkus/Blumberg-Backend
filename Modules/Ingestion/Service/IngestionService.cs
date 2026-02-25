using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
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

        for (var i = 0; i < readings.Count; i++)
        {
            var item = readings[i];
            var (accepted, rejectionReason) = await ValidateAndBuildReadingAsync(organizationId, item, cancellationToken);
            if (accepted != null)
            {
                acceptedReadings.Add(accepted);
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

        await ingestionRunRepository.CreateRunAsync(run, rejectedReadings, acceptedReadings, cancellationToken);

        logger.LogInformation("Ingestion run {RunId} completed: {Accepted} accepted, {Rejected} rejected",
            run.Id, run.AcceptedRecords, run.RejectedRecords);

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

    private async Task<(SensorReading? Accepted, string? RejectionReason)> ValidateAndBuildReadingAsync(
        Guid organizationId,
        IngestReadingItem item,
        CancellationToken cancellationToken)
    {
        var sensor = await sensorRepository.GetByIdAsync(item.SensorId);
        if (sensor == null)
            return (null, "Sensor not found or access denied");
        if (sensor.OrganizationId != organizationId)
            return (null, "Sensor not found or access denied");

        if (!Enum.IsDefined(typeof(Unit), item.Unit))
            return (null, "Invalid unit");

        var reading = new SensorReading
        {
            SensorId = item.SensorId,
            Value = item.Value,
            TimestampUtc = item.TimestampUtc,
            Unit = item.Unit,
            OrganizationId = organizationId
        };
        return (reading, null);
    }
}
