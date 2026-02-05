namespace Adapters.Permissions.Repositories;

/// <summary>
/// Repository interface for RBAC operations using Casbin
/// </summary>
public interface IRbacRepository
{
    // Policy Management
    Task<bool> AddPolicyAsync(string role, string resource, string action);
    Task<bool> RemovePolicyAsync(string role, string resource, string action);
    Task<List<List<string>>> GetPoliciesForRoleAsync(string role);
    Task<List<List<string>>> GetAllPoliciesAsync();

    // Role Assignment
    Task<bool> AddRoleForUserAsync(string userId, string role);
    Task<bool> RemoveRoleForUserAsync(string userId, string role);
    Task<bool> RemoveAllRolesForUserAsync(string userId);
    Task<List<string>> GetRolesForUserAsync(string userId);
    Task<List<string>> GetUsersForRoleAsync(string role);
    Task<List<string>> GetAllRolesAsync();
    Task<bool> HasRoleForUserAsync(string userId, string role);

    // Permission Enforcement
    Task<bool> EnforceAsync(string userId, string resource, string action);
    Task<List<bool>> BatchEnforceAsync(List<List<string>> requests);

    // Policy Reload
    Task LoadPolicyAsync();
    Task SavePolicyAsync();
}

