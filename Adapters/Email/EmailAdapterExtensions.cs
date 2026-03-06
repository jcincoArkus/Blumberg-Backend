using Adapters.Config;
using Microsoft.Extensions.DependencyInjection;
using Shared.Notifications;

namespace Adapters.Email;

/// <summary>
/// DI registration for alert email notifications. Registers IAlertNotificationSender based on AppConfig.Email.Provider (None or Smtp).
/// </summary>
public static class EmailAdapterExtensions
{
    /// <summary>
    /// Registers IAlertNotificationSender: NoOp when Provider is None, SmtpAlertNotificationSender when Provider is Smtp.
    /// Call before registering modules that depend on IAlertNotificationSender (e.g. Alerts).
    /// </summary>
    public static IServiceCollection AddAlertEmailNotifications(this IServiceCollection services, AppConfig config)
    {
        services.AddSingleton(config.Email);
        if (config.Email.IsSmtp)
        {
            services.AddScoped<IAlertNotificationSender, SmtpAlertNotificationSender>();
        }
        else
        {
            services.AddScoped<IAlertNotificationSender, NoOpAlertNotificationSender>();
        }
        return services;
    }
}
