using Modules.Permissions.Dto;

namespace Modules.Permissions.Service;

/// <summary>
/// Service for managing role-permission assignments
/// </summary>
public interface IRolePermissionService
{
    /// <summary>
    /// Get all permissions for a role
    /// </summary>
    Task<List<RolePermissionDto>> GetRolePermissionsAsync(string roleName);

    /// <summary>
    /// Assign permissions to a role
    /// </summary>
    Task AssignPermissionsToRoleAsync(string roleName, List<RolePermissionDto> permissions);

    /// <summary>
    /// Remove permissions from a role
    /// </summary>
    Task RemovePermissionsFromRoleAsync(string roleName, List<RolePermissionDto> permissions);

    /// <summary>
    /// Replace all permissions for a role
    /// </summary>
    Task ReplaceRolePermissionsAsync(string roleName, List<RolePermissionDto> permissions);
}

