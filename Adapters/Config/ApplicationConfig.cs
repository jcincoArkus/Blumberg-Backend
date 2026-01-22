namespace Adapters.Config;

/// <summary>
/// General application configuration
/// </summary>
/// <remarks>
/// Configures application-level settings like environment and URLs.
/// Values can be set via ASPNETCORE_ prefixed environment variables.
/// </remarks>
public class ApplicationConfig
{
    /// <summary>
    /// Application environment name
    /// </summary>
    /// <remarks>
    /// Standard values: Development, Staging, Production
    /// Affects logging, error handling, and feature availability
    /// </remarks>
    /// <example>Development, Staging, Production</example>
    public string Environment { get; private set; } = "Development";

    /// <summary>
    /// URLs the application listens on
    /// </summary>
    /// <remarks>
    /// Can be multiple URLs separated by semicolon.
    /// Format: protocol://host:port
    /// </remarks>
    /// <example>http://localhost:5000, https://0.0.0.0:5001;http://0.0.0.0:5000</example>
    public string[] Urls { get; private set; } = ["http://localhost:5000"];

    /// <summary>
    /// Application name
    /// </summary>
    /// <example>Blumberg API</example>
    public string Name { get; private set; } = "Blumberg API";

    /// <summary>
    /// Application version
    /// </summary>
    /// <example>1.0.0</example>
    public string Version { get; private set; } = "0.0.0";

    /// <summary>
    /// Initializes configuration from environment variables
    /// </summary>
    public ApplicationConfig Init()
    {
        Environment = EnvHelper.GetEnv("ASPNETCORE_ENVIRONMENT", "Development");
        Urls = EnvHelper.GetEnv("ASPNETCORE_URLS", "http://localhost:5000").Split(';');
        Name = EnvHelper.GetEnv("APP_NAME", "Blumberg API");
        Version = EnvHelper.GetEnv("APP_VERSION", "0.0.0");

        return this;
    }

    /// <summary>
    /// Checks if running in Development environment
    /// </summary>
    public bool IsDevelopment => Environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if running in Production environment
    /// </summary>
    public bool IsProduction => Environment.Equals("Production", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if running in Staging environment
    /// </summary>
    public bool IsStaging => Environment.Equals("Staging", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Validates application configuration
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid</exception>
    public ApplicationConfig Validate()
    {
        if (string.IsNullOrWhiteSpace(Environment))
            throw new InvalidOperationException("Application environment (ASPNETCORE_ENVIRONMENT) is required");

        if (Urls == null || Urls.Length == 0)
            throw new InvalidOperationException("At least one URL must be configured (ASPNETCORE_URLS)");

        // Validate URL format
        foreach (var url in Urls)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                throw new InvalidOperationException($"Invalid URL format: {url}");
        }

        return this;
    }
}

