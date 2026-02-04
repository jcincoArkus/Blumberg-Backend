using Adapters.Database;
using Microsoft.EntityFrameworkCore;
using Shared.Entity;

namespace Modules.Sites.Repository;

/// <summary>
/// Repository implementation for Site entity operations
/// </summary>
public class SiteRepository(ApplicationDbContext context) : ISiteRepository
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc />
    public async Task<List<Site>> GetAllAsync()
    {
        return await _context.Sites
            .Where(s => s.DeletedAt == null)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Site?> GetByIdAsync(Guid id)
    {
        return await _context.Sites
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null);
    }

    /// <inheritdoc />
    public async Task<Site> CreateAsync(Site site)
    {
        site.Id = Guid.NewGuid();
        site.CreatedAt = DateTime.UtcNow;
        site.UpdatedAt = null;
        site.DeletedAt = null;

        _context.Sites.Add(site);
        await _context.SaveChangesAsync();

        return site;
    }

    /// <inheritdoc />
    public async Task<Site> UpdateAsync(Site site)
    {
        site.UpdatedAt = DateTime.UtcNow;

        _context.Sites.Update(site);
        await _context.SaveChangesAsync();

        return site;
    }

    /// <inheritdoc />
    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var site = await GetByIdAsync(id);
        if (site == null)
            return false;

        site.DeletedAt = DateTime.UtcNow;
        site.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}