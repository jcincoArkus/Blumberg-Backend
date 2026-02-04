using Microsoft.Extensions.DependencyInjection;
using Modules.Sensors.Repository;
using Modules.Sensors.Service;

namespace Modules.Sensors;

/// <summary>
/// Extension methods for registering Sensors module services
/// </summary>
public static class SensorModule
{
    /// <summary>
    /// Adds Sensors module services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddSensorModule(this IServiceCollection services)
    {
        services.AddScoped<ISensorRepository, SensorRepository>();
        services.AddScoped<ISensorService, SensorService>();

        return services;
    }
}
