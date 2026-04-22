using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Sites.Repository;

/// <summary>
/// Repository implementation for Site entity operations
/// </summary>
public class SiteRepository(ApplicationDbContext context, ILogger<SiteRepository> logger) : ISiteRepository
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<Site> Items, int TotalCount)> GetPagedAsync(PaginationRequest request)
    {
        logger.LogDebug("Querying sites page {Page}, pageSize {PageSize}, search '{Search}'", request.Page, request.PageSize, request.Search);

        IQueryable<Site> query = context.Sites
            .Where(s => s.DeletedAt == null)
            .Include(s => s.Organization);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(s => s.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} sites from database (total: {TotalCount})", items.Count, totalCount);

        return (items, totalCount);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Site?> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Querying site by ID: {Id}", id);

        var site = await context.Sites
            .Include(s => s.Organization)
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null);

        if (site != null)
            logger.LogInformation("Found site {Id}", id);
        else
            logger.LogDebug("Site not found with ID: {Id}", id);

        return site;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Site> CreateAsync(Site site)
    {
        logger.LogDebug("Creating site in database: {Name}", site.Name);

        site.Id = Guid.NewGuid();
        site.CreatedAt = DateTime.UtcNow;
        site.UpdatedAt = null;
        site.DeletedAt = null;

        context.Sites.Add(site);
        await context.SaveChangesAsync();

        logger.LogInformation("Site created in database with ID: {Id}", site.Id);

        return site;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Site> UpdateAsync(Site site)
    {
        logger.LogDebug("Updating site in database: {Id}", site.Id);

        site.UpdatedAt = DateTime.UtcNow;

        context.Sites.Update(site);
        await context.SaveChangesAsync();

        logger.LogInformation("Site {Id} updated in database", site.Id);

        return site;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<bool> SoftDeleteAsync(Guid id)
    {
        logger.LogDebug("Soft deleting site in database: {Id}", id);

        var site = await GetByIdAsync(id);
        if (site == null)
        {
            logger.LogWarning("Site not found for deletion: {Id}", id);
            return false;
        }

        site.DeletedAt = DateTime.UtcNow;
        site.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Site {Id} soft deleted in database", id);

        return true;
    }
}
