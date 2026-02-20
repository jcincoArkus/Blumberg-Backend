using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Ingestion.Dto;
using Shared.Entity;

namespace Modules.Ingestion.Repository;

/// <summary>
/// Repository implementation for ingestion run operations
/// </summary>
public class IngestionRunRepository(ApplicationDbContext context, ILogger<IngestionRunRepository> logger) : IIngestionRunRepository
{
    /// <inheritdoc />
    [Span]
    public virtual async Task CreateRunAsync(
        IngestionRun run,
        IReadOnlyList<IngestionRejectedReading> rejectedReadings,
        IReadOnlyList<SensorReading> acceptedReadings,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Creating ingestion run with {Rejected} rejected and {Accepted} accepted readings",
            rejectedReadings.Count, acceptedReadings.Count);

        if (run.Id == Guid.Empty)
            run.Id = Guid.NewGuid();

        run.CreatedAt = DateTime.UtcNow;
        run.UpdatedAt = null;
        run.DeletedAt = null;

        foreach (var r in rejectedReadings)
        {
            r.Id = Guid.NewGuid();
            r.IngestionRunId = run.Id;
        }
        run.RejectedReadings = rejectedReadings.ToList();

        foreach (var reading in acceptedReadings)
        {
            reading.Id = Guid.NewGuid();
            reading.IngestionRunId = run.Id;
            reading.CreatedAt = DateTime.UtcNow;
            reading.UpdatedAt = null;
            reading.DeletedAt = null;
        }

        context.IngestionRuns.Add(run);
        context.SensorReadings.AddRange(acceptedReadings);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created ingestion run {RunId} with {Accepted} accepted, {Rejected} rejected",
            run.Id, run.AcceptedRecords, run.RejectedRecords);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<IngestionRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting ingestion run {Id}", id);

        var run = await context.IngestionRuns
            .Include(e => e.SensorReadings)
            .Include(e => e.RejectedReadings)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null, cancellationToken);

        if (run != null)
            logger.LogInformation("Found ingestion run {Id}", id);

        return run;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<IngestionRun> Items, int TotalCount)> GetPagedAsync(
        GetIngestionRunsRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Querying ingestion runs page {Page}, pageSize {PageSize}", request.Page, request.PageSize);

        IQueryable<IngestionRun> query = context.IngestionRuns
            .Where(e => e.DeletedAt == null);

        if (request.Status.HasValue)
            query = query.Where(e => e.Status == request.Status.Value);
        if (request.Source.HasValue)
            query = query.Where(e => e.Source == request.Source.Value);
        if (request.From.HasValue)
            query = query.Where(e => e.CreatedAt >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(e => e.CreatedAt <= request.To.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} ingestion runs (total: {TotalCount})", items.Count, totalCount);

        return (items, totalCount);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<IngestionStatsResponse> GetStatsAsync(
        GetIngestionRunsRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting ingestion stats for date range");

        IQueryable<IngestionRun> query = context.IngestionRuns
            .Where(e => e.DeletedAt == null);

        if (request.Status.HasValue)
            query = query.Where(e => e.Status == request.Status.Value);
        if (request.Source.HasValue)
            query = query.Where(e => e.Source == request.Source.Value);
        if (request.From.HasValue)
            query = query.Where(e => e.CreatedAt >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(e => e.CreatedAt <= request.To.Value);

        var stats = await query
            .GroupBy(e => 1)
            .Select(g => new IngestionStatsResponse
            {
                TotalRecords = g.Sum(e => e.TotalRecords),
                AcceptedRecords = g.Sum(e => e.AcceptedRecords),
                RejectedRecords = g.Sum(e => e.RejectedRecords),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return stats ?? new IngestionStatsResponse();
    }
}
