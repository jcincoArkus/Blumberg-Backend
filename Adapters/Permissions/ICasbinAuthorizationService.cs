namespace Adapters.Permissions;

/// <summary>
/// Service for checking permissions using Casbin
/// </summary>
public interface ICasbinAuthorizationService
{
    /// <summary>
    /// Check if a user has permission to perform an action on a resource
    /// </summary>
    /// <param name="userEmail">User's email address</param>
    /// <param name="resource">Resource name (e.g., "equipment", "alerts")</param>
    /// <param name="action">Action name (e.g., "read", "write", "acknowledge")</param>
    /// <returns>True if user has permission, false otherwise</returns>
    Task<bool> CheckPermissionAsync(string userEmail, string resource, string action);

    /// <summary>
    /// Assign a role to a user
    /// </summary>
    /// <param name="userEmail">User's email address</param>
    /// <param name="role">Role name (e.g., "admin", "standard", "readonly")</param>
    Task AssignRoleAsync(string userEmail, string role);

    /// <summary>
    /// Remove a role from a user
    /// </summary>
    /// <param name="userEmail">User's email address</param>
    /// <param name="role">Role name</param>
    Task RemoveRoleAsync(string userEmail, string role);

    /// <summary>
    /// Get all roles assigned to a user
    /// </summary>
    /// <param name="userEmail">User's email address</param>
    /// <returns>List of role names</returns>
    Task<List<string>> GetUserRolesAsync(string userEmail);

    /// <summary>
    /// Get all permissions for a role
    /// </summary>
    /// <param name="role">Role name</param>
    /// <returns>List of permissions (resource, action)</returns>
    Task<List<(string Resource, string Action)>> GetRolePermissionsAsync(string role);

    /// <summary>
    /// Reload policies from database
    /// </summary>
    Task ReloadPoliciesAsync();
}

