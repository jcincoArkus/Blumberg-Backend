using System.Text;
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
    /// Adds JWT authentication to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="secretKey">JWT secret key (min 32 characters)</param>
    /// <param name="issuer">JWT issuer</param>
    /// <param name="audience">JWT audience</param>
    /// <param name="expirationHours">Token expiration in hours (default: 24)</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        string secretKey,
        string issuer,
        string audience,
        int expirationHours = 24)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
            throw new ArgumentException("JWT secret key cannot be null or empty", nameof(secretKey));

        if (secretKey.Length < 32)
            throw new ArgumentException("JWT secret key must be at least 32 characters long", nameof(secretKey));

        // Register JWT service
        services.AddScoped<IJwtService>(sp =>
            new JwtService(secretKey, issuer, audience, expirationHours));

        // Configure authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
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

        return services;
    }
}

