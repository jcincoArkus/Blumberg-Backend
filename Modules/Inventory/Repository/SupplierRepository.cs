using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public class SupplierRepository(ApplicationDbContext context, ILogger<SupplierRepository> logger) : ISupplierRepository
{
    [Span]
    public virtual async Task<(IReadOnlyList<Supplier> Items, int TotalCount)> GetPagedAsync(PaginationRequest request)
    {
        IQueryable<Supplier> query = context.Suppliers;

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

        logger.LogInformation("Retrieved {Count} suppliers (total: {TotalCount})", items.Count, totalCount);
        return (items, totalCount);
    }

    [Span]
    public virtual async Task<Supplier?> GetByIdAsync(Guid id)
    {
        return await context.Suppliers.FirstOrDefaultAsync(e => e.Id == id);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<Supplier> CreateAsync(Supplier entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        context.Suppliers.Add(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Created supplier {Id}", entity.Id);
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<Supplier> UpdateAsync(Supplier entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Suppliers.Update(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Updated supplier {Id}", entity.Id);
        return entity;
    }

    [Span]
    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;
        context.Suppliers.Remove(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Deleted supplier {Id}", id);
        return true;
    }
}
