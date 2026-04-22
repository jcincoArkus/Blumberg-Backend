using Modules.Sites.Dto;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Sites.Service;

/// <summary>
/// Service interface for site operations
/// </summary>
public interface ISiteService
{
    /// <summary>
    /// Gets paginated sites
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated sites and total count</returns>
    Task<(IReadOnlyList<Site> Items, int TotalCount)> GetAllAsync(PaginationRequest request);

    /// <summary>
    /// Gets a site by ID
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <returns>Site entity</returns>
    /// <exception cref="KeyNotFoundException">When site is not found</exception>
    Task<Site> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new site
    /// </summary>
    /// <param name="request">Site creation request</param>
    /// <returns>Created site entity</returns>
    /// <exception cref="InvalidOperationException">When site creation fails</exception>
    Task<Site> CreateAsync(SiteRequest request);

    /// <summary>
    /// Updates an existing site
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <param name="request">Site update request</param>
    /// <returns>Updated site entity</returns>
    /// <exception cref="KeyNotFoundException">When site is not found</exception>
    /// <exception cref="InvalidOperationException">When site update fails</exception>
    Task<Site> UpdateAsync(Guid id, SiteRequest request);

    /// <summary>
    /// Soft deletes a site
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <exception cref="KeyNotFoundException">When site is not found</exception>
    Task DeleteAsync(Guid id);
}
