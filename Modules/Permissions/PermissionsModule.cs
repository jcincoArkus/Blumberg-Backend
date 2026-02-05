using Adapters.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Modules.Permissions.Service;

namespace Modules.Permissions;

/// <summary>
/// Extension methods for registering Permissions module services
/// </summary>
public static class PermissionsModule
{
    /// <summary>
    /// Adds Permissions module services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddPermissionsModule(this IServiceCollection services)
    {
        // Register services
        services.AddScopedWithSpan<IRoleService, RoleService>();
        services.AddScopedWithSpan<IRolePermissionService, RolePermissionService>();
        services.AddScopedWithSpan<IUserRoleService, UserRoleService>();
        services.AddScopedWithSpan<IPermissionQueryService, PermissionQueryService>();

        return services;
    }
}

