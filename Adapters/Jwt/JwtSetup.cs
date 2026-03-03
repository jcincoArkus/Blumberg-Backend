using System.Text;
using Adapters.Config;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Adapters.Jwt;

/// <summary>
/// Extension methods for setting up JWT authentication
/// </summary>
public static class JwtSetup
{
    /// <summary>
    /// Adds JWT authentication to the service collection using config from ConfigLoader
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Authentication builder for chaining (e.g. AddApiKeyAuthentication)</returns>
    public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection services)
    {
        var config = ConfigLoader.Load();
        return services.AddJwtAuthentication(config);
    }

    /// <summary>
    /// Adds JWT authentication to the service collection using provided config
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="config">Application configuration</param>
    /// <returns>Authentication builder for chaining</returns>
    public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection services, AppConfig config)
    {
        var jwt = config.Jwt;

        if (string.IsNullOrWhiteSpace(jwt.SecretKey))
            throw new ArgumentException("JWT secret key cannot be null or empty");

        if (jwt.SecretKey.Length < 32)
            throw new ArgumentException("JWT secret key must be at least 32 characters long");

        // Register JWT service
        services.AddScoped<IJwtService>(sp =>
            new JwtService(jwt.SecretKey, jwt.Issuer, jwt.Audience, jwt.ExpirationHours, jwt.RefreshExpirationDays));

        // Configure authentication (return builder so caller can add ApiKey scheme, etc.)
        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Preserve custom claim names (e.g. "orgId") so TenantContext can read them
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers["Token-Expired"] = "true";
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();
        return authBuilder;
    }
}

