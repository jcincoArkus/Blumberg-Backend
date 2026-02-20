using Adapters.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Modules.Ingestion.Repository;
using Modules.Ingestion.Service;

namespace Modules.Ingestion;

/// <summary>
/// Extension methods for registering Ingestion module services
/// </summary>
public static class IngestionModule
{
    /// <summary>
    /// Adds Ingestion module services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddIngestionModule(this IServiceCollection services)
    {
        services.AddScopedWithSpan<IIngestionRunRepository, IngestionRunRepository>();
        services.AddScopedWithSpan<IApiKeyRepository, ApiKeyRepository>();
        services.AddScopedWithSpan<IIngestionService, IngestionService>();
        services.AddScopedWithSpan<IApiKeyService, ApiKeyService>();

        return services;
    }
}
