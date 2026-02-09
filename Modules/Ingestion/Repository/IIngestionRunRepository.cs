using Modules.Ingestion.Dto;

namespace Modules.Ingestion.Repository;

/// <summary>
/// Repository interface for ingestion run operations
/// </summary>
public interface IIngestionRunRepository
{
    /// <summary>
    /// Creates an ingestion run with its rejected readings and accepted sensor readings in one transaction
    /// </summary>
    /// <param name="run">The run entity (with Id set); will be updated with AcceptedRecords, RejectedRecords, Status, CompletedAt after insert</param>
    /// <param name="rejectedReadings">Rejected readings to attach to the run</param>
    /// <param name="acceptedReadings">Sensor readings to insert (with IngestionRunId set to run.Id)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CreateRunAsync(
        Shared.Entity.IngestionRun run,
        IReadOnlyList<Shared.Entity.IngestionRejectedReading> rejectedReadings,
        IReadOnlyList<Shared.Entity.SensorReading> acceptedReadings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an ingestion run by ID with accepted and rejected readings included
    /// </summary>
    /// <param name="id">Run ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Run or null if not found</returns>
    Task<Shared.Entity.IngestionRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated ingestion runs with optional filters
    /// </summary>
    /// <param name="request">Query parameters (pagination, status, source, date range)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Runs and total count</returns>
    Task<(IReadOnlyList<Shared.Entity.IngestionRun> Items, int TotalCount)> GetPagedAsync(
        GetIngestionRunsRequest request,
        CancellationToken cancellationToken = default);
}
