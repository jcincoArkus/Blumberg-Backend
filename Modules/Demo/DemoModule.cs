using Microsoft.Extensions.DependencyInjection;

namespace Modules.Demo;

/// <summary>
/// Registers the demo-data background service. No-op unless DEMO_MODE=true.
/// </summary>
public static class DemoModule
{
    public static IServiceCollection AddDemoModule(this IServiceCollection services)
    {
        if (DemoMode.IsEnabled)
            services.AddHostedService<DemoDataHostedService>();
        return services;
    }
}
