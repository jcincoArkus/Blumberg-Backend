namespace Modules.Permissions.Dto;

/// <summary>
/// Role information with metadata
/// </summary>
public record RoleDto
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required string Color { get; init; }
    public required bool IsDefault { get; init; }
}

/// <summary>
/// Request to create a new role
/// </summary>
public record CreateRoleRequest
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required string Color { get; init; }
    public bool IsDefault { get; init; }
}

/// <summary>
/// Request to update an existing role
/// </summary>
public record UpdateRoleRequest
{
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required string Color { get; init; }
    public bool IsDefault { get; init; }
}

/// <summary>
/// Response after creating or updating a role
/// </summary>
public record RoleResponse
{
    public required RoleDto Role { get; init; }
}

/// <summary>
/// List of roles
/// </summary>
public record RolesListResponse
{
    public required List<RoleDto> Roles { get; init; }
}

