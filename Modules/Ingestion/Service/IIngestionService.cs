using Modules.Ingestion.Dto;

namespace Modules.Ingestion.Service;

/// <summary>
/// Service interface for ingestion operations
/// </summary>
public interface IIngestionService
{
    /// <summary>
    /// Accepts a batch of readings, creates an ingestion run, validates and processes each reading
    /// </summary>
    /// <param name="organizationId">Current organization (from tenant context)</param>
    /// <param name="readings">Batch of readings to ingest (1 to 5000 items)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created ingestion run entity</returns>
    /// <exception cref="InvalidOperationException">When batch is empty or exceeds max size</exception>
    Task<Shared.Entity.IngestionRun> IngestReadingsAsync(
        Guid organizationId,
        IReadOnlyList<IngestReadingItem> readings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists ingestion runs (paginated, filterable by status, source, date range)
    /// </summary>
    /// <param name="organizationId">Current organization</param>
    /// <param name="request">Query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated runs and total count</returns>
    Task<(IReadOnlyList<Shared.Entity.IngestionRun> Items, int TotalCount)> GetRunsAsync(
        Guid organizationId,
        GetIngestionRunsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single ingestion run by ID with accepted and rejected readings
    /// </summary>
    /// <param name="organizationId">Current organization</param>
    /// <param name="id">Run ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Run with per-reading results</returns>
    /// <exception cref="KeyNotFoundException">When run is not found</exception>
    Task<Shared.Entity.IngestionRun> GetRunByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets aggregated stats (total/accepted/rejected records) for runs matching the filter (e.g. last 24h)
    /// </summary>
    /// <param name="organizationId">Current organization</param>
    /// <param name="request">Filter (status, source, date range)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aggregated stats</returns>
    Task<Modules.Ingestion.Dto.IngestionStatsResponse> GetStatsAsync(
        Guid organizationId,
        GetIngestionRunsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of rejected readings for a sensor in runs within the time range (for per-sensor tracing).
    /// </summary>
    /// <param name="organizationId">Current organization</param>
    /// <param name="sensorId">Sensor to get rejection count for</param>
    /// <param name="request">Time range (From, To)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sensor ID and rejection count in the range</returns>
    Task<SensorRejectionCountResponse> GetSensorRejectionCountAsync(
        Guid organizationId,
        Guid sensorId,
        GetSensorRejectionCountRequest request,
        CancellationToken cancellationToken = default);
}
