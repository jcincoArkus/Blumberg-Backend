using Adapters.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Modules.Auth.Repository;
using Modules.Auth.Service;
using Shared.Notifications;

namespace Modules.Auth;

/// <summary>
/// Extension methods for registering Auth module services
/// </summary>
public static class AuthModule
{
    /// <summary>
    /// Adds Auth module services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        services.AddScopedWithSpan<IAdminRepository, AdminRepository>();
        services.AddScopedWithSpan<IAuthService, AuthService>();
        services.AddScopedWithSpan<IAlertRecipientResolver, AlertRecipientResolver>();

        return services;
    }
}

