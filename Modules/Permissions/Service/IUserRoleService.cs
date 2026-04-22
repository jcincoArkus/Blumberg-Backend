using Modules.Permissions.Dto;

namespace Modules.Permissions.Service;

/// <summary>
/// Service for managing user-role assignments
/// </summary>
public interface IUserRoleService
{
    /// <summary>
    /// Get all roles for a user
    /// </summary>
    Task<List<string>> GetUserRolesAsync(string userId);

    /// <summary>
    /// Get all users with a specific role
    /// </summary>
    Task<List<string>> GetUsersForRoleAsync(string roleName);

    /// <summary>
    /// Assign roles to a user
    /// </summary>
    Task AssignRolesToUserAsync(string userId, List<string> roles);

    /// <summary>
    /// Remove roles from a user
    /// </summary>
    Task RemoveRolesFromUserAsync(string userId, List<string> roles);

    /// <summary>
    /// Replace all roles for a user
    /// </summary>
    Task ReplaceUserRolesAsync(string userId, List<string> roles);
}

