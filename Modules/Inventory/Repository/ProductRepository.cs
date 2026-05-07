using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public class ProductRepository(ApplicationDbContext context, ILogger<ProductRepository> logger) : IProductRepository
{
    [Span]
    public virtual async Task<(IReadOnlyList<InventoryProduct> Items, int TotalCount)> GetPagedAsync(GetProductsRequest request)
    {
        IQueryable<InventoryProduct> query = context.InventoryProducts.Include(e => e.Category);

        if (request.CategoryId.HasValue)
            query = query.Where(e => e.CategoryId == request.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(request.Unit))
            query = query.Where(e => e.Unit == request.Unit);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(e => e.Name.ToLower().Contains(search) || e.Sku.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} products (total: {TotalCount})", items.Count, totalCount);
        return (items, totalCount);
    }

    [Span]
    public virtual async Task<InventoryProduct?> GetByIdAsync(Guid id)
    {
        return await context.InventoryProducts
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<InventoryProduct> CreateAsync(InventoryProduct entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        context.InventoryProducts.Add(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Created product {Id} ({Sku})", entity.Id, entity.Sku);
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<InventoryProduct> UpdateAsync(InventoryProduct entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.InventoryProducts.Update(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Updated product {Id}", entity.Id);
        return entity;
    }

    [Span]
    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await context.InventoryProducts.FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null) return false;
        context.InventoryProducts.Remove(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Deleted product {Id}", id);
        return true;
    }

    [Span]
    public virtual async Task<bool> ExistsBySkuAsync(string sku, Guid? excludeId = null)
    {
        var query = context.InventoryProducts.Where(e => e.Sku == sku);
        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);
        return await query.AnyAsync();
    }
}
