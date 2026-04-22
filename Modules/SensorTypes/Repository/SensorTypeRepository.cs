using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Dto;
using Shared.Entity;

namespace Modules.SensorTypes.Repository;

/// <summary>
/// Repository implementation for SensorType entity operations
/// </summary>
public class SensorTypeRepository(ApplicationDbContext context, ILogger<SensorTypeRepository> logger) : ISensorTypeRepository
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<SensorType> Items, int TotalCount)> GetPagedAsync(PaginationRequest request)
    {
        IQueryable<SensorType> query = context.SensorTypes.Where(e => e.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(e => e.Type.ToString().ToLower().Contains(search) || e.Unit.ToString().ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} sensor types (total: {TotalCount})", items.Count, totalCount);
        return (items, totalCount);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<SensorType?> GetByIdAsync(Guid id)
    {
        return await context.SensorTypes
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<SensorType> CreateAsync(SensorType entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.DeletedAt = null;
        context.SensorTypes.Add(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Created sensor type {Id}", entity.Id);
        return entity;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<SensorType> UpdateAsync(SensorType entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.SensorTypes.Update(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Updated sensor type {Id}", entity.Id);
        return entity;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<bool> SoftDeleteAsync(Guid id)
    {
        var entity = await context.SensorTypes.FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);
        if (entity == null) return false;
        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        logger.LogInformation("Soft deleted sensor type {Id}", id);
        return true;
    }
}
