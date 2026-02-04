using Modules.Sites.Dto;

namespace Modules.Sites.Service;

/// <summary>
/// Service interface for site operations
/// </summary>
public interface ISiteService
{
    /// <summary>
    /// Gets all sites
    /// </summary>
    /// <returns>List of site responses</returns>
    Task<List<SiteResponse>> GetAllAsync();

    /// <summary>
    /// Gets a site by ID
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <returns>Site response</returns>
    /// <exception cref="KeyNotFoundException">When site is not found</exception>
    Task<SiteResponse> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new site
    /// </summary>
    /// <param name="request">Site creation request</param>
    /// <returns>Created site response</returns>
    /// <exception cref="InvalidOperationException">When site creation fails</exception>
    Task<SiteResponse> CreateAsync(SiteRequest request);

    /// <summary>
    /// Updates an existing site
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <param name="request">Site update request</param>
    /// <returns>Updated site response</returns>
    /// <exception cref="KeyNotFoundException">When site is not found</exception>
    /// <exception cref="InvalidOperationException">When site update fails</exception>
    Task<SiteResponse> UpdateAsync(Guid id, SiteRequest request);

    /// <summary>
    /// Soft deletes a site
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <exception cref="KeyNotFoundException">When site is not found</exception>
    Task DeleteAsync(Guid id);
}