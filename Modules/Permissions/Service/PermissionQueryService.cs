using Adapters.Permissions.Repositories;
using Microsoft.Extensions.Logging;
using Modules.Permissions.Dto;
using Shared.Permissions;
using PermissionAction = Shared.Permissions.Action;

namespace Modules.Permissions.Service;

/// <summary>
/// Service for querying available permissions, resources, and actions
/// </summary>
public class PermissionQueryService(
    IRbacRepository rbacRepository,
    ILogger<PermissionQueryService> logger) : IPermissionQueryService
{
    public Task<List<ResourceDto>> GetAllResourcesAsync()
    {
        logger.LogDebug("Getting all available resources");

        // Use the generated Resources class to get all available resources
        var resources = new List<ResourceDto>();

        // Get all resource properties from the Resources static class
        var resourceType = typeof(Resources);
        var resourceProperties = resourceType.GetProperties(
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.Static);

        foreach (var prop in resourceProperties)
        {
            if (!prop.PropertyType.GetInterfaces().Contains(typeof(IResource))) continue;
            
            if (prop.GetValue(null) is IResource resource)
            {
                resources.Add(new ResourceDto
                {
                    Name = resource.Name,
                    Actions = resource.GetActions().Select(a => (string)a).ToList()
                });
            }
        }

        return Task.FromResult(resources);
    }

    public Task<List<string>> GetAllActionsAsync()
    {
        logger.LogDebug("Getting all available actions");

        // Use the generated Actions class to get all available actions
        var actionsType = typeof(Actions);
        var actionProperties = actionsType.GetProperties(
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.Static);

        var actions = actionProperties
            .Where(p => p.PropertyType == typeof(PermissionAction))
            .Select(p => (string)((PermissionAction)p.GetValue(null)!))
            .ToList();

        return Task.FromResult(actions);
    }

    public async Task<bool> CheckUserPermissionAsync(string userId, string resource, string action)
    {
        logger.LogDebug("Checking permission for user {UserId}: {Resource}:{Action}", userId, resource, action);
        return await rbacRepository.EnforceAsync(userId, resource, action);
    }
}

