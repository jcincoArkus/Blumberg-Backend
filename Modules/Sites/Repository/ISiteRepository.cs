using Shared.Dto;
using Shared.Entity;

namespace Modules.Sites.Repository;

/// <summary>
/// Repository interface for Site entity operations
/// </summary>
public interface ISiteRepository
{
    /// <summary>
    /// Gets paginated sites
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated sites and total count</returns>
    Task<(IReadOnlyList<Site> Items, int TotalCount)> GetPagedAsync(PaginationRequest request);

    /// <summary>
    /// Gets a site by ID
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <returns>Site entity or null if not found</returns>
    Task<Site?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new site
    /// </summary>
    /// <param name="site">Site entity to create</param>
    /// <returns>Created site entity</returns>
    Task<Site> CreateAsync(Site site);

    /// <summary>
    /// Updates an existing site
    /// </summary>
    /// <param name="site">Site entity to update</param>
    /// <returns>Updated site entity</returns>
    Task<Site> UpdateAsync(Site site);

    /// <summary>
    /// Soft deletes a site by setting DeletedAt timestamp
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <returns>True if site was found and deleted, false otherwise</returns>
    Task<bool> SoftDeleteAsync(Guid id);
}
