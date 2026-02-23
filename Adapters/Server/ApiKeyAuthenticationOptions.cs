using Microsoft.AspNetCore.Authentication;

namespace Adapters.Server;

/// <summary>
/// Options for API key authentication
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// HTTP header name for the API key (default: X-Api-Key)
    /// </summary>
    public const string DefaultHeaderName = "X-Api-Key";

    /// <summary>
    /// Header name to read the API key from
    /// </summary>
    public string HeaderName { get; set; } = DefaultHeaderName;
}
