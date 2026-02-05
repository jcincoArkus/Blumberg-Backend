using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;

namespace Modules.Sites.Repository;

/// <summary>
/// Repository implementation for Site entity operations
/// </summary>
public class SiteRepository(ApplicationDbContext context, ILogger<SiteRepository> logger) : ISiteRepository
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc />
    [Span]
    public virtual async Task<List<Site>> GetAllAsync()
    {
        logger.LogDebug("Querying all sites");

        var sites = await _context.Sites
            .Where(s => s.DeletedAt == null)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} sites from database", sites.Count);

        return sites;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Site?> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Querying site by ID: {Id}", id);

        var site = await _context.Sites
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

        _context.Sites.Add(site);
        await _context.SaveChangesAsync();

        logger.LogInformation("Site created in database with ID: {Id}", site.Id);

        return site;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Site> UpdateAsync(Site site)
    {
        logger.LogDebug("Updating site in database: {Id}", site.Id);

        site.UpdatedAt = DateTime.UtcNow;

        _context.Sites.Update(site);
        await _context.SaveChangesAsync();

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

        await _context.SaveChangesAsync();

        logger.LogInformation("Site {Id} soft deleted in database", id);

        return true;
    }
}