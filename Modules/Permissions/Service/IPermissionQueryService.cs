using Modules.Permissions.Dto;

namespace Modules.Permissions.Service;

/// <summary>
/// Service for querying available permissions, resources, and actions
/// </summary>
public interface IPermissionQueryService
{
    /// <summary>
    /// Get all available resources with their actions
    /// </summary>
    Task<List<ResourceDto>> GetAllResourcesAsync();

    /// <summary>
    /// Get all available actions
    /// </summary>
    Task<List<string>> GetAllActionsAsync();

    /// <summary>
    /// Check if a user has a specific permission
    /// </summary>
    Task<bool> CheckUserPermissionAsync(string userId, string resource, string action);
}

