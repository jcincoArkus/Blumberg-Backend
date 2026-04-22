using Adapters.Permissions.Repositories;
using Microsoft.Extensions.Logging;
using Modules.Permissions.Dto;

namespace Modules.Permissions.Service;

/// <summary>
/// Service for managing role-permission assignments
/// </summary>
public class RolePermissionService(
    IRbacRepository rbacRepository,
    ILogger<RolePermissionService> logger) : IRolePermissionService
{
    public async Task<List<RolePermissionDto>> GetRolePermissionsAsync(string roleName)
    {
        logger.LogDebug("Getting permissions for role: {RoleName}", roleName);
        var policies = await rbacRepository.GetPoliciesForRoleAsync(roleName);

        // Casbin policies are in format: [role, resource, action]
        return policies
            .Where(p => p.Count >= 3)
            .Select(p => new RolePermissionDto
            {
                Resource = p[1], // resource is at index 1
                Action = p[2]    // action is at index 2
            }).ToList();
    }

    public async Task AssignPermissionsToRoleAsync(string roleName, List<RolePermissionDto> permissions)
    {
        logger.LogInformation("Assigning {Count} permissions to role: {RoleName}", permissions.Count, roleName);

        foreach (var permission in permissions)
        {
            await rbacRepository.AddPolicyAsync(roleName, permission.Resource, permission.Action);
        }

        logger.LogInformation("Successfully assigned permissions to role: {RoleName}", roleName);
    }

    public async Task RemovePermissionsFromRoleAsync(string roleName, List<RolePermissionDto> permissions)
    {
        logger.LogInformation("Removing {Count} permissions from role: {RoleName}", permissions.Count, roleName);

        foreach (var permission in permissions)
        {
            await rbacRepository.RemovePolicyAsync(roleName, permission.Resource, permission.Action);
        }

        logger.LogInformation("Successfully removed permissions from role: {RoleName}", roleName);
    }

    public async Task ReplaceRolePermissionsAsync(string roleName, List<RolePermissionDto> permissions)
    {
        logger.LogInformation("Replacing all permissions for role: {RoleName}", roleName);

        // Get existing permissions
        var existingPolicies = await rbacRepository.GetPoliciesForRoleAsync(roleName);

        // Remove all existing permissions
        foreach (var policy in existingPolicies.Where(p => p.Count >= 3))
        {
            await rbacRepository.RemovePolicyAsync(roleName, policy[1], policy[2]);
        }

        // Add new permissions
        foreach (var permission in permissions)
        {
            await rbacRepository.AddPolicyAsync(roleName, permission.Resource, permission.Action);
        }

        logger.LogInformation("Successfully replaced permissions for role: {RoleName}", roleName);
    }
}

