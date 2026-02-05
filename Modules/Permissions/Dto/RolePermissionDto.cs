namespace Modules.Permissions.Dto;

/// <summary>
/// Permission assigned to a role
/// </summary>
public record RolePermissionDto
{
    public required string Resource { get; init; }
    public required string Action { get; init; }
}

/// <summary>
/// Request to assign permissions to a role
/// </summary>
public record AssignPermissionsToRoleRequest
{
    public required List<RolePermissionDto> Permissions { get; init; }
}

/// <summary>
/// Request to remove permissions from a role
/// </summary>
public record RemovePermissionsFromRoleRequest
{
    public required List<RolePermissionDto> Permissions { get; init; }
}

/// <summary>
/// Response with role's permissions
/// </summary>
public record RolePermissionsResponse
{
    public required string RoleName { get; init; }
    public required List<RolePermissionDto> Permissions { get; init; }
}

