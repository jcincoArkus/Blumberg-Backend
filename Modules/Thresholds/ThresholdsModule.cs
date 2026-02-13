using Adapters.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Modules.Thresholds.Repository;
using Modules.Thresholds.Service;

namespace Modules.Thresholds;

/// <summary>
/// Extension methods for registering Thresholds module services
/// </summary>
public static class ThresholdsModule
{
    /// <summary>
    /// Adds Thresholds module services to the service collection
    /// </summary>
    public static IServiceCollection AddThresholdsModule(this IServiceCollection services)
    {
        services.AddScopedWithSpan<IThresholdRepository, ThresholdRepository>();
        services.AddScopedWithSpan<IThresholdService, ThresholdService>();
        return services;
    }
}
