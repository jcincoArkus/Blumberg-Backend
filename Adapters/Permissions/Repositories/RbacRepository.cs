using Casbin;
using Microsoft.Extensions.Logging;

namespace Adapters.Permissions.Repositories;

/// <summary>
/// Repository implementation for RBAC operations using Casbin
/// </summary>
public class RbacRepository(IEnforcer enforcer, ILogger<RbacRepository> logger) : IRbacRepository
{
    // Policy Management
    public async Task<bool> AddPolicyAsync(string role, string resource, string action)
    {
        logger.LogDebug("Adding policy: {Role} -> {Resource} -> {Action}", role, resource, action);
        var added = await enforcer.AddPolicyAsync(role, resource, action);
        if (added)
            await enforcer.LoadPolicyAsync(); // Reload to sync
        return added;
    }

    public async Task<bool> RemovePolicyAsync(string role, string resource, string action)
    {
        logger.LogDebug("Removing policy: {Role} -> {Resource} -> {Action}", role, resource, action);
        var removed = await enforcer.RemovePolicyAsync(role, resource, action);
        if (removed)
            await enforcer.LoadPolicyAsync(); // Reload to sync
        return removed;
    }

    public Task<List<List<string>>> GetPoliciesForRoleAsync(string role)
    {
        logger.LogDebug("Getting policies for role: {Role}", role);
        var policies = enforcer.GetFilteredPolicy(0, role);
        return Task.FromResult(policies.Select(p => p.ToList()).ToList());
    }

    public Task<List<List<string>>> GetAllPoliciesAsync()
    {
        logger.LogDebug("Getting all policies");
        var policies = enforcer.GetPolicy();
        return Task.FromResult(policies.Select(p => p.ToList()).ToList());
    }

    // Role Assignment
    public async Task<bool> AddRoleForUserAsync(string userId, string role)
    {
        logger.LogDebug("Adding role {Role} to user {UserId}", role, userId);
        var added = await enforcer.AddRoleForUserAsync(userId, role);
        if (added)
            await enforcer.LoadPolicyAsync(); // Reload to sync
        return added;
    }

    public async Task<bool> RemoveRoleForUserAsync(string userId, string role)
    {
        logger.LogDebug("Removing role {Role} from user {UserId}", role, userId);
        var removed = await enforcer.DeleteRoleForUserAsync(userId, role);
        if (removed)
            await enforcer.LoadPolicyAsync(); // Reload to sync
        return removed;
    }

    public async Task<bool> RemoveAllRolesForUserAsync(string userId)
    {
        logger.LogDebug("Removing all roles from user {UserId}", userId);
        var removed = await enforcer.DeleteRolesForUserAsync(userId);
        if (removed)
            await enforcer.LoadPolicyAsync(); // Reload to sync
        return removed;
    }

    public Task<List<string>> GetRolesForUserAsync(string userId)
    {
        logger.LogDebug("Getting roles for user {UserId}", userId);
        var roles = enforcer.GetRolesForUser(userId);
        return Task.FromResult(roles.ToList());
    }

    public Task<List<string>> GetUsersForRoleAsync(string role)
    {
        logger.LogDebug("Getting users for role {Role}", role);
        var users = enforcer.GetUsersForRole(role);
        return Task.FromResult(users.ToList());
    }

    public Task<List<string>> GetAllRolesAsync()
    {
        logger.LogDebug("Getting all roles");
        var roles = enforcer.GetAllRoles();
        return Task.FromResult(roles.ToList());
    }

    public Task<bool> HasRoleForUserAsync(string userId, string role)
    {
        logger.LogDebug("Checking if user {UserId} has role {Role}", userId, role);
        return Task.FromResult(enforcer.HasRoleForUser(userId, role));
    }

    // Permission Enforcement
    public Task<bool> EnforceAsync(string userId, string resource, string action)
    {
        logger.LogDebug("Enforcing permission: {UserId} -> {Resource} -> {Action}", userId, resource, action);
        return Task.FromResult(enforcer.Enforce(userId, resource, action));
    }

    public Task<List<bool>> BatchEnforceAsync(List<List<string>> requests)
    {
        logger.LogDebug("Batch enforcing {Count} requests", requests.Count);
        var results = new List<bool>();
        foreach (var request in requests)
        {
            if (request.Count >= 3)
            {
                var result = enforcer.Enforce(request[0], request[1], request[2]);
                results.Add(result);
            }
            else
            {
                results.Add(false);
            }
        }
        return Task.FromResult(results);
    }

    // Policy Reload
    public Task LoadPolicyAsync()
    {
        logger.LogDebug("Loading policies from database");
        enforcer.LoadPolicy();
        return Task.CompletedTask;
    }

    public Task SavePolicyAsync()
    {
        logger.LogDebug("Saving policies to database");
        enforcer.SavePolicy();
        return Task.CompletedTask;
    }
}

