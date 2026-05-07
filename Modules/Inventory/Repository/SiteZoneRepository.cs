using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public class SiteZoneRepository(ApplicationDbContext context, ILogger<SiteZoneRepository> logger) : ISiteZoneRepository
{
    [Span]
    public virtual async Task<(IReadOnlyList<SiteZone> Items, int TotalCount)> GetPagedAsync(PaginationRequest request, Guid? siteId)
    {
        IQueryable<SiteZone> query = context.SiteZones.Include(e => e.Site);

        if (siteId.HasValue)
            query = query.Where(e => e.SiteId == siteId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(e => e.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.SiteId).ThenBy(e => e.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} site zones (total: {TotalCount})", items.Count, totalCount);
        return (items, totalCount);
    }

    [Span]
    public virtual async Task<SiteZone?> GetByIdAsync(Guid id)
    {
        return await context.SiteZones.Include(e => e.Site).FirstOrDefaultAsync(e => e.Id == id);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<SiteZone> CreateAsync(SiteZone entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        context.SiteZones.Add(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Created site zone {Id}", entity.Id);
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<SiteZone> UpdateAsync(SiteZone entity)
    {
        context.SiteZones.Update(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Updated site zone {Id}", entity.Id);
        return entity;
    }

    [Span]
    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await context.SiteZones.FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null) return false;
        context.SiteZones.Remove(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Deleted site zone {Id}", id);
        return true;
    }
}
