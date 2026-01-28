using Shared.Entity;

namespace Modules.Auth.Repository;

/// <summary>
/// Repository interface for Admin entity operations
/// </summary>
public interface IAdminRepository
{
    /// <summary>
    /// Gets an admin by ID
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <returns>Admin entity or null if not found</returns>
    Task<Admin?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets an admin by email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>Admin entity or null if not found</returns>
    Task<Admin?> GetByEmailAsync(string email);

    /// <summary>
    /// Checks if an admin with the given email exists
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsAsync(string email);

    /// <summary>
    /// Gets all active admins
    /// </summary>
    /// <returns>List of active admin entities</returns>
    Task<List<Admin>> GetAllAsync();

    /// <summary>
    /// Creates a new admin
    /// </summary>
    /// <param name="admin">Admin entity</param>
    /// <returns>Created admin entity</returns>
    Task<Admin> CreateAsync(Admin admin);

    /// <summary>
    /// Updates an admin
    /// </summary>
    /// <param name="admin">Admin entity</param>
    /// <returns>Updated admin entity</returns>
    Task<Admin> UpdateAsync(Admin admin);

    /// <summary>
    /// Soft deletes an admin by setting the DeletedAt timestamp
    /// </summary>
    /// <param name="admin">Admin entity</param>
    /// <returns>Soft deleted admin entity</returns>
    Task<Admin> DeleteAsync(Admin admin);
}

