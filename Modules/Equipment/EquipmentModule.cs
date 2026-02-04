using Microsoft.Extensions.DependencyInjection;
using Modules.Equipment.Repository;
using Modules.Equipment.Service;

namespace Modules.Equipment;

/// <summary>
/// Extension methods for registering Equipment module services
/// </summary>
public static class EquipmentModule
{
    /// <summary>
    /// Adds Equipment module services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddEquipmentModule(this IServiceCollection services)
    {
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IEquipmentService, EquipmentService>();

        return services;
    }
}
