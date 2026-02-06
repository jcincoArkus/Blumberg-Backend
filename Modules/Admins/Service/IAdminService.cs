using Modules.Admins.Dto;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Admins.Service;

/// <summary>
/// Service interface for admin operations
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Gets paginated admins
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated admins and total count</returns>
    Task<(IReadOnlyList<Admin> Items, int TotalCount)> GetAllAsync(PaginationRequest request);

    /// <summary>
    /// Gets an admin by ID
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <returns>Admin entity</returns>
    Task<Admin> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new admin
    /// </summary>
    /// <param name="admin">Admin information</param>
    /// <returns>Created admin entity</returns>
    Task<Admin> CreateAsync(AdminRequest admin);

    /// <summary>
    /// Updates an admin
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <param name="admin">Admin information</param>
    /// <returns>Updated admin entity</returns>
    Task<Admin> UpdateAsync(Guid id, AdminRequest admin);

    /// <summary>
    /// Deletes an admin
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <returns>Void</returns>
    Task DeleteAsync(Guid id);
}
