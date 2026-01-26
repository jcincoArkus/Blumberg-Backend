using Modules.Admins.Dto;

namespace Modules.Admins.Service;

/// <summary>
/// Service interface for admin operations
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Gets all active admins
    /// </summary>
    /// <returns>List of admin responses</returns>
    Task<List<AdminResponse>> GetAllAsync();

    /// <summary>
    /// Gets an admin by ID
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <returns>Admin response</returns>
    Task<AdminResponse> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new admin
    /// </summary>
    /// <param name="admin">Admin information</param>
    /// <returns>Admin response</returns>
    Task<AdminResponse> CreateAsync(AdminRequest admin);

    /// <summary>
    /// Updates an admin
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <param name="admin">Admin information</param>
    /// <returns>Admin response</returns>
    Task<AdminResponse> UpdateAsync(Guid id, AdminRequest admin);

    /// <summary>
    /// Deletes an admin
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <returns>Void</returns>
    Task DeleteAsync(Guid id);

}
