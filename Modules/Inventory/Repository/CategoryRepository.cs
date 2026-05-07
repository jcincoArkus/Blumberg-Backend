using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public class CategoryRepository(ApplicationDbContext context, ILogger<CategoryRepository> logger) : ICategoryRepository
{
    [Span]
    public virtual async Task<(IReadOnlyList<InventoryCategory> Items, int TotalCount)> GetPagedAsync(PaginationRequest request)
    {
        IQueryable<InventoryCategory> query = context.InventoryCategories;

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

        logger.LogInformation("Retrieved {Count} categories (total: {TotalCount})", items.Count, totalCount);
        return (items, totalCount);
    }

    [Span]
    public virtual async Task<InventoryCategory?> GetByIdAsync(Guid id)
    {
        return await context.InventoryCategories.FirstOrDefaultAsync(e => e.Id == id);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<InventoryCategory> CreateAsync(InventoryCategory entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        context.InventoryCategories.Add(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Created category {Id}", entity.Id);
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<InventoryCategory> UpdateAsync(InventoryCategory entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.InventoryCategories.Update(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Updated category {Id}", entity.Id);
        return entity;
    }

    [Span]
    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;
        context.InventoryCategories.Remove(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Deleted category {Id}", id);
        return true;
    }
}
