using Adapters.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Modules.Sites.Repository;
using Modules.Sites.Service;

namespace Modules.Sites;

/// <summary>
/// Extension methods for registering Sites module services
/// </summary>
public static class SitesModule
{
    /// <summary>
    /// Adds Sites module services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddSitesModule(this IServiceCollection services)
    {
        services.AddScopedWithSpan<ISiteRepository, SiteRepository>();
        services.AddScopedWithSpan<ISiteService, SiteService>();

        return services;
    }
}