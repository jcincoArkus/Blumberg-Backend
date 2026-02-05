using Adapters.Permissions.Repositories;
using Microsoft.Extensions.Logging;

namespace Modules.Permissions.Service;

/// <summary>
/// Service for managing user-role assignments
/// </summary>
public class UserRoleService(
    IRbacRepository rbacRepository,
    ILogger<UserRoleService> logger) : IUserRoleService
{
    public async Task<List<string>> GetUserRolesAsync(string userId)
    {
        logger.LogDebug("Getting roles for user: {UserId}", userId);
        return await rbacRepository.GetRolesForUserAsync(userId);
    }

    public async Task<List<string>> GetUsersForRoleAsync(string roleName)
    {
        logger.LogDebug("Getting users for role: {RoleName}", roleName);
        return await rbacRepository.GetUsersForRoleAsync(roleName);
    }

    public async Task AssignRolesToUserAsync(string userId, List<string> roles)
    {
        logger.LogInformation("Assigning {Count} roles to user: {UserId}", roles.Count, userId);

        foreach (var role in roles)
        {
            await rbacRepository.AddRoleForUserAsync(userId, role);
        }

        logger.LogInformation("Successfully assigned roles to user: {UserId}", userId);
    }

    public async Task RemoveRolesFromUserAsync(string userId, List<string> roles)
    {
        logger.LogInformation("Removing {Count} roles from user: {UserId}", roles.Count, userId);

        foreach (var role in roles)
        {
            await rbacRepository.RemoveRoleForUserAsync(userId, role);
        }

        logger.LogInformation("Successfully removed roles from user: {UserId}", userId);
    }

    public async Task ReplaceUserRolesAsync(string userId, List<string> roles)
    {
        logger.LogInformation("Replacing all roles for user: {UserId}", userId);

        // Remove all existing roles
        await rbacRepository.RemoveAllRolesForUserAsync(userId);

        // Add new roles
        foreach (var role in roles)
        {
            await rbacRepository.AddRoleForUserAsync(userId, role);
        }

        logger.LogInformation("Successfully replaced roles for user: {UserId}", userId);
    }
}

