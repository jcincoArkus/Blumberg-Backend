using Microsoft.AspNetCore.Authentication;

namespace Adapters.Server;

/// <summary>
/// Extension methods for adding API key authentication
/// </summary>
public static class ApiKeyAuthenticationExtensions
{
    /// <summary>
    /// Adds API key authentication (X-Api-Key header). Use with Bearer on endpoints that accept both (e.g. ingestion).
    /// </summary>
    public static AuthenticationBuilder AddApiKeyAuthentication(this AuthenticationBuilder builder)
    {
        return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", _ => { });
    }
}
