using Casbin;
using Microsoft.Extensions.Logging;

namespace Adapters.Permissions;

/// <summary>
/// Implementation of Casbin authorization service
/// </summary>
public class CasbinAuthorizationService(IEnforcer enforcer, ILogger<CasbinAuthorizationService> logger) 
    : ICasbinAuthorizationService
{
    private readonly IEnforcer _enforcer = enforcer;
    private readonly ILogger<CasbinAuthorizationService> _logger = logger;

    /// <inheritdoc/>
    public async Task<bool> CheckPermissionAsync(string userEmail, string resource, string action)
    {
        try
        {
            _logger.LogDebug("Checking permission: user={UserEmail}, resource={Resource}, action={Action}", 
                userEmail, resource, action);

            var allowed = await _enforcer.EnforceAsync(userEmail, resource, action);

            _logger.LogDebug("Permission check result: {Allowed}", allowed);

            return allowed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for user {UserEmail}", userEmail);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task AssignRoleAsync(string userEmail, string role)
    {
        try
        {
            _logger.LogInformation("Assigning role {Role} to user {UserEmail}", role, userEmail);

            await _enforcer.AddRoleForUserAsync(userEmail, role);

            _logger.LogInformation("Successfully assigned role {Role} to user {UserEmail}", role, userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {Role} to user {UserEmail}", role, userEmail);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task RemoveRoleAsync(string userEmail, string role)
    {
        try
        {
            _logger.LogInformation("Removing role {Role} from user {UserEmail}", role, userEmail);

            await _enforcer.DeleteRoleForUserAsync(userEmail, role);

            _logger.LogInformation("Successfully removed role {Role} from user {UserEmail}", role, userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {Role} from user {UserEmail}", role, userEmail);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetUserRolesAsync(string userEmail)
    {
        try
        {
            _logger.LogDebug("Getting roles for user {UserEmail}", userEmail);

            // Use synchronous method - Casbin.NET 2.x doesn't have async role methods
            var roles = _enforcer.GetRolesForUser(userEmail).ToList();

            _logger.LogDebug("User {UserEmail} has {RoleCount} roles", userEmail, roles.Count);

            return await Task.FromResult(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user {UserEmail}", userEmail);
            return new List<string>();
        }
    }

    /// <inheritdoc/>
    public async Task<List<(string Resource, string Action)>> GetRolePermissionsAsync(string role)
    {
        try
        {
            _logger.LogDebug("Getting permissions for role {Role}", role);

            // Use synchronous method - Casbin.NET 2.x doesn't have async permission methods
            var permissions = _enforcer.GetPermissionsForUser(role);

            var result = permissions
                .Select(p => {
                    var parts = p.ToList();
                    return (Resource: parts.Count > 1 ? parts[1] : "", Action: parts.Count > 2 ? parts[2] : "");
                })
                .ToList();

            _logger.LogDebug("Role {Role} has {PermissionCount} permissions", role, result.Count);

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for role {Role}", role);
            return new List<(string, string)>();
        }
    }

    /// <inheritdoc/>
    public async Task ReloadPoliciesAsync()
    {
        try
        {
            _logger.LogInformation("Reloading Casbin policies from database");

            await _enforcer.LoadPolicyAsync();

            _logger.LogInformation("Successfully reloaded Casbin policies");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading Casbin policies");
            throw;
        }
    }
}

