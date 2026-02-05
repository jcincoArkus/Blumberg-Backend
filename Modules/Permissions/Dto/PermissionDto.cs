namespace Modules.Permissions.Dto;

/// <summary>
/// Available action
/// </summary>
public record ActionDto
{
    public required string Name { get; init; }
}

/// <summary>
/// Resource with its available actions
/// </summary>
public record ResourceDto
{
    public required string Name { get; init; }
    public required List<string> Actions { get; init; }
}

/// <summary>
/// Response with all available resources
/// </summary>
public record ResourcesResponse
{
    public required List<ResourceDto> Resources { get; init; }
}

/// <summary>
/// Response with all available actions
/// </summary>
public record ActionsResponse
{
    public required List<string> Actions { get; init; }
}

/// <summary>
/// Request to check if user has permission
/// </summary>
public record CheckPermissionRequest
{
    public required string Resource { get; init; }
    public required string Action { get; init; }
}

/// <summary>
/// Response for permission check
/// </summary>
public record CheckPermissionResponse
{
    public required bool HasPermission { get; init; }
}

