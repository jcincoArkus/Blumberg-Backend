using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public class InventorySiteRepository(ApplicationDbContext context, ILogger<InventorySiteRepository> logger) : IInventorySiteRepository
{
    [Span]
    public virtual async Task<(IReadOnlyList<InventorySite> Items, int TotalCount)> GetPagedAsync(PaginationRequest request)
    {
        IQueryable<InventorySite> query = context.InventorySites;

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(e => e.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} inventory sites (total: {TotalCount})", items.Count, totalCount);
        return (items, totalCount);
    }

    [Span]
    public virtual async Task<InventorySite?> GetByIdAsync(Guid id)
    {
        return await context.InventorySites.FirstOrDefaultAsync(e => e.Id == id);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<InventorySite> CreateAsync(InventorySite entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        context.InventorySites.Add(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Created inventory site {Id}", entity.Id);
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<InventorySite> UpdateAsync(InventorySite entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.InventorySites.Update(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Updated inventory site {Id}", entity.Id);
        return entity;
    }

    [Span]
    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;
        context.InventorySites.Remove(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Deleted inventory site {Id}", id);
        return true;
    }
}
