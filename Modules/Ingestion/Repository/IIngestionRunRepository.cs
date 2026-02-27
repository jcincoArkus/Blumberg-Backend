using Modules.Ingestion.Dto;

namespace Modules.Ingestion.Repository;

/// <summary>
/// Repository interface for ingestion run operations
/// </summary>
public interface IIngestionRunRepository
{
    /// <summary>
    /// Creates an ingestion run with its rejected readings, accepted sensor readings, and new alerts in one transaction.
    /// Optionally updates sensor breach state (FirstOutOfRangeAt) for duration-based alert validation.
    /// </summary>
    /// <param name="run">The run entity (with Id set)</param>
    /// <param name="rejectedReadings">Rejected readings to attach to the run</param>
    /// <param name="acceptedReadings">Sensor readings to insert (with IngestionRunId set to run.Id)</param>
    /// <param name="newAlerts">New alerts to create atomically with the run</param>
    /// <param name="sensorBreachStateUpdates">Optional map of sensor Id to FirstOutOfRangeAt (null = in range)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CreateRunAsync(
        Shared.Entity.IngestionRun run,
        IReadOnlyList<Shared.Entity.IngestionRejectedReading> rejectedReadings,
        IReadOnlyList<Shared.Entity.SensorReading> acceptedReadings,
        IReadOnlyList<Shared.Entity.Alert> newAlerts,
        IReadOnlyDictionary<Guid, DateTime?>? sensorBreachStateUpdates = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an ingestion run by ID with accepted and rejected readings included
    /// </summary>
    /// <param name="id">Run ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Run or null if not found</returns>
    Task<Shared.Entity.IngestionRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated ingestion runs with optional filters (scoped to organization).
    /// </summary>
    /// <param name="organizationId">Organization scope</param>
    /// <param name="request">Query parameters (pagination, status, source, date range)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Runs and total count</returns>
    Task<(IReadOnlyList<Shared.Entity.IngestionRun> Items, int TotalCount)> GetPagedAsync(
        Guid organizationId,
        GetIngestionRunsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets aggregated stats (total/accepted/rejected records) for runs matching the filter (scoped to organization).
    /// </summary>
    /// <param name="organizationId">Organization scope</param>
    /// <param name="request">Filter (status, source, date range); pagination is ignored</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aggregated stats</returns>
    Task<Modules.Ingestion.Dto.IngestionStatsResponse> GetStatsAsync(
        Guid organizationId,
        GetIngestionRunsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of rejected readings for a given sensor in runs within the time range (run CreatedAt).
    /// Only rejections with SensorId set (e.g. invalid sensor id, invalid unit) are traced per sensor.
    /// </summary>
    /// <param name="organizationId">Organization scope</param>
    /// <param name="sensorId">Sensor to count rejections for</param>
    /// <param name="from">Start of range (UTC, inclusive)</param>
    /// <param name="to">End of range (UTC, inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of rejected readings for that sensor in the range</returns>
    Task<int> GetRejectedCountBySensorAsync(
        Guid organizationId,
        Guid sensorId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
