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
    /// <param name="readings">Batch of readings to ingest</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Run ID and summary (total, accepted, rejected, status)</returns>
    Task<IngestReadingsResponse> IngestReadingsAsync(
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
}
