using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Modules.Auth;

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

        return services;
    }

    /// <summary>
    /// Gets all assemblies containing controllers from application modules
    /// </summary>
    /// <returns>List of assemblies with controllers</returns>
    public static IEnumerable<Assembly> GetControllerAssemblies()
    {
        yield return typeof(Auth.Controller.AuthController).Assembly;
    }
}

