using Adapters.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Modules.Alerts.Repository;
using Modules.Alerts.Service;
using Shared.Notifications;

namespace Modules.Alerts;

/// <summary>
/// Extension methods for registering Alerts module services
/// </summary>
public static class AlertsModule
{
    /// <summary>
    /// Adds Alerts module services to the service collection
    /// </summary>
    public static IServiceCollection AddAlertsModule(this IServiceCollection services)
    {
        services.AddScopedWithSpan<IAlertRepository, AlertRepository>();
        services.AddScopedWithSpan<IRecommendedActionRepository, RecommendedActionRepository>();
        services.AddScopedWithSpan<IAlertService, AlertService>();
        services.AddScopedWithSpan<IAlertTriggeredNotifier, AlertTriggeredNotifier>();

        // Email queue worker (SQS + SES) used by endpoints:
        // - POST /api/queue-email
        // - POST /api/process-queue
        // Uses only Environment.GetEnvironmentVariable() inside the service.
        services.AddSingleton<SqsSesEmailQueueService>();

        return services;
    }
}
