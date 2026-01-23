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
}

