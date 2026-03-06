using Microsoft.Extensions.DependencyInjection;

namespace Adapters.Config;

/// <summary>
/// Extension methods for setting up configuration
/// </summary>
public static class ConfigSetup
{
    /// <summary>
    /// Adds application configuration to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="envFilePath">Optional custom .env file path</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddAppConfiguration(
        this IServiceCollection services,
        string? envFilePath = null)
    {
        // Load configuration
        var config = ConfigLoader.Load(envFilePath);

        // Register as singleton
        services.AddSingleton(config);
        services.AddSingleton(config.Database);
        services.AddSingleton(config.Jwt);
        services.AddSingleton(config.Application);
        services.AddSingleton(config.Telemetry);
        services.AddSingleton(config.Email);

        return services;
    }

    /// <summary>
    /// Gets the loaded configuration
    /// </summary>
    public static AppConfig GetAppConfig(this IServiceProvider services)
    {
        return services.GetRequiredService<AppConfig>();
    }
}

