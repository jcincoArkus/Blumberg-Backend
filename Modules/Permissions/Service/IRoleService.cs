using Modules.Permissions.Dto;

namespace Modules.Permissions.Service;

/// <summary>
/// Service for managing roles
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Get all roles with metadata
    /// </summary>
    Task<List<RoleDto>> GetAllRolesAsync();

    /// <summary>
    /// Get a specific role by name
    /// </summary>
    Task<RoleDto?> GetRoleByNameAsync(string roleName);

    /// <summary>
    /// Create a new role
    /// </summary>
    Task<RoleDto> CreateRoleAsync(CreateRoleRequest request);

    /// <summary>
    /// Update an existing role
    /// </summary>
    Task<RoleDto> UpdateRoleAsync(string roleName, UpdateRoleRequest request);

    /// <summary>
    /// Delete a role
    /// </summary>
    Task DeleteRoleAsync(string roleName);
}

