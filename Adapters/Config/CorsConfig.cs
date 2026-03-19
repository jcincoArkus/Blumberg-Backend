namespace Adapters.Config;

/// <summary>
/// CORS configuration for cross-origin requests (e.g. frontend hosted on Amplify).
/// </summary>
/// <remarks>
/// Environment variable: CORS_ORIGINS (comma-separated origins).
/// CORS_ORIGINS is required in all environments (fail closed).
/// </remarks>
public class CorsConfig
{
    /// <summary>
    /// Allowed origins (CORS_ORIGINS).
    /// </summary>
    public string[] AllowedOrigins { get; private set; } = [];

    /// <summary>
    /// Initializes from CORS_ORIGINS (comma-separated list).
    /// </summary>
    public CorsConfig Init()
    {
        var value = EnvHelper.GetEnvRequired("CORS_ORIGINS");

        AllowedOrigins = string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (AllowedOrigins.Length == 0)
            throw new InvalidOperationException("CORS_ORIGINS must contain at least one origin");

        return this;
    }
}
