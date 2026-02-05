using Adapters.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Modules.Admins.Service;

namespace Modules.Admins;

/// <summary>
/// Extension methods for registering Admins module services
/// </summary>
public static class AdminsModule
{
    /// <summary>
    /// Adds Admins module services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddAdminsModule(this IServiceCollection services)
    {
        services.AddScopedWithSpan<IAdminService, AdminService>();

        return services;
    }
}
