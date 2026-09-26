using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public class LotRepository(ApplicationDbContext context, ILogger<LotRepository> logger) : ILotRepository
{
    [Span]
    public virtual async Task<(IReadOnlyList<Lot> Items, int TotalCount)> GetPagedAsync(GetLotsRequest request)
    {
        IQueryable<Lot> query = context.Lots
            .Include(e => e.Product)
            .Include(e => e.Site)
            .Include(e => e.Supplier);

        if (request.ProductId.HasValue)
            query = query.Where(e => e.ProductId == request.ProductId.Value);

        if (request.SiteId.HasValue)
            query = query.Where(e => e.SiteId == request.SiteId.Value);

        if (request.ExpiringBefore.HasValue)
            query = query.Where(e => e.ExpiresAt < request.ExpiringBefore.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(e => e.LotCode.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.EntryAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} lots (total: {TotalCount})", items.Count, totalCount);
        return (items, totalCount);
    }

    [Span]
    public virtual async Task<Lot?> GetByCodeAsync(string lotCode)
    {
        return await context.Lots
            .Include(e => e.Product)
            .Include(e => e.Site)
            .Include(e => e.Supplier)
            .FirstOrDefaultAsync(e => e.LotCode == lotCode);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<Lot> CreateAsync(Lot entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        context.Lots.Add(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Created lot {LotCode}", entity.LotCode);
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<Lot> UpdateAsync(Lot entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Lots.Update(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Updated lot {LotCode}", entity.LotCode);
        return entity;
    }

    [Span]
    public virtual async Task<bool> DeleteAsync(string lotCode)
    {
        var entity = await context.Lots.FirstOrDefaultAsync(e => e.LotCode == lotCode);
        if (entity == null) return false;
        context.Lots.Remove(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Deleted lot {LotCode}", lotCode);
        return true;
    }

    [Span]
    public virtual async Task<bool> ExistsByCodeAsync(string lotCode)
    {
        return await context.Lots.AnyAsync(e => e.LotCode == lotCode);
    }
}
