using Adapters.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Modules.SensorTypes.Repository;
using Modules.SensorTypes.Service;

namespace Modules.SensorTypes;

/// <summary>
/// Extension methods for registering SensorTypes module services
/// </summary>
public static class SensorTypesModule
{
    /// <summary>
    /// Adds SensorTypes module services to the service collection
    /// </summary>
    public static IServiceCollection AddSensorTypesModule(this IServiceCollection services)
    {
        services.AddScopedWithSpan<ISensorTypeRepository, SensorTypeRepository>();
        services.AddScopedWithSpan<ISensorTypeService, SensorTypeService>();
        return services;
    }
}
