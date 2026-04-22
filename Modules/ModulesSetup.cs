using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Modules.Admins;
using Modules.Alerts;
using Modules.Auth;
using Modules.Equipment;
using Modules.Ingestion;
using Modules.Permissions;
using Modules.Sensors;
using Modules.SensorTypes;
using Modules.Sites;
using Modules.Thresholds;

namespace Modules;

/// <summary>
/// Central registration for all application modules
/// </summary>
public static class ModulesSetup
{
    /// <summary>
    /// Registers all application modules and their controllers
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddApplicationModules(this IServiceCollection services)
    {
        services.AddAuthModule();
        services.AddAdminsModule();
        services.AddSitesModule();
        services.AddEquipmentModule();
        services.AddSensorModule();
        services.AddSensorTypesModule();
        services.AddThresholdsModule();
        services.AddAlertsModule();
        services.AddIngestionModule();
        services.AddPermissionsModule();
        return services;
    }

    /// <summary>
    /// Gets all assemblies containing controllers from application modules
    /// </summary>
    /// <returns>List of assemblies with controllers</returns>
    public static IEnumerable<Assembly> GetControllerAssemblies()
    {
        yield return typeof(Auth.Controller.AuthController).Assembly;
        yield return typeof(Admins.Controller.AdminController).Assembly;
        yield return typeof(Sites.Controller.SiteController).Assembly;
        yield return typeof(Equipment.Controller.EquipmentController).Assembly;
        yield return typeof(Sensors.Controller.SensorController).Assembly;
        yield return typeof(Ingestion.Controller.IngestionController).Assembly;
        yield return typeof(Permissions.Controller.RolesController).Assembly;
        yield return typeof(Alerts.Controller.AlertsController).Assembly;
    }
}

