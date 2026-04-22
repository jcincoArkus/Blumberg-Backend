using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Thresholds.Repository;

/// <summary>
/// Repository implementation for Threshold entity operations
/// </summary>
public class ThresholdRepository(ApplicationDbContext context, ILogger<ThresholdRepository> logger) : IThresholdRepository
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<Threshold> Items, int TotalCount)> GetPagedAsync(PaginationRequest request)
    {
        IQueryable<Threshold> query = context.Thresholds.Where(e => e.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            if (decimal.TryParse(search, out var value))
                query = query.Where(e => e.Min == value || e.Max == value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} thresholds (total: {TotalCount})", items.Count, totalCount);
        return (items, totalCount);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Threshold?> GetByIdAsync(Guid id)
    {
        return await context.Thresholds
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Threshold> CreateAsync(Threshold entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.DeletedAt = null;
        context.Thresholds.Add(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Created threshold {Id}", entity.Id);
        return entity;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Threshold> UpdateAsync(Threshold entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Thresholds.Update(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Updated threshold {Id}", entity.Id);
        return entity;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<bool> SoftDeleteAsync(Guid id)
    {
        var entity = await context.Thresholds.FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);
        if (entity == null) return false;
        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        logger.LogInformation("Soft deleted threshold {Id}", id);
        return true;
    }
}
