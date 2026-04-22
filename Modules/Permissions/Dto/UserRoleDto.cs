namespace Modules.Permissions.Dto;

/// <summary>
/// Request to assign roles to a user
/// </summary>
public record AssignRolesToUserRequest
{
    public required List<string> Roles { get; init; }
}

/// <summary>
/// Request to remove roles from a user
/// </summary>
public record RemoveRolesFromUserRequest
{
    public required List<string> Roles { get; init; }
}

/// <summary>
/// Response with user's roles
/// </summary>
public record UserRolesResponse
{
    public required string UserId { get; init; }
    public required List<string> Roles { get; init; }
}

/// <summary>
/// Response with users assigned to a role
/// </summary>
public record RoleUsersResponse
{
    public required string RoleName { get; init; }
    public required List<string> UserIds { get; init; }
}

