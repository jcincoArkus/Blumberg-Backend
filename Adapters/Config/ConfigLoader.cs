using DotNetEnv;

namespace Adapters.Config;

/// <summary>
/// Configuration loader with .env file support
/// </summary>
public static class ConfigLoader
{
    private static bool _loaded = false;
    private static AppConfig? _config;

    /// <summary>
    /// Loads configuration from environment variables and .env file
    /// </summary>
    /// <param name="envFilePath">Optional custom .env file path</param>
    /// <returns>Loaded and validated configuration</returns>
    public static AppConfig Load(string? envFilePath = null)
    {
        if (_loaded && _config != null)
            return _config;

        // Load .env file
        LoadEnvFile(envFilePath);

        // Build configuration - each config initializes and validates itself
        _config = new AppConfig
        {
            Database = new DatabaseConfig().Init().Validate(),
            Jwt = new JwtConfig().Init().Validate(),
            Application = new ApplicationConfig().Init().Validate(),
            Log = new LogConfig().Init().Validate(),
            Telemetry = new TelemetryConfig().Init().Validate(),
            Cors = new CorsConfig().Init(),
            Email = new EmailConfig().Init().Validate()
        };

        _loaded = true;
        return _config;
    }

    /// <summary>
    /// Loads configuration for CLI commands that only need database access (e.g. nukeAndPave, migrations).
    /// JWT and other API-only settings are not required, so the task can run in ECS without JWT_SECRET_KEY.
    /// </summary>
    public static AppConfig LoadForDatabaseOperations(string? envFilePath = null)
    {
        if (_loaded && _config != null)
            return _config;

        LoadEnvFile(envFilePath);

        _config = new AppConfig
        {
            Database = new DatabaseConfig().Init().Validate(),
            Jwt = new JwtConfig().Init(requireSecretKey: false).Validate(requireSecret: false),
            Application = new ApplicationConfig().Init().Validate(),
            Log = new LogConfig().Init().Validate(),
            Telemetry = new TelemetryConfig().Init().Validate(),
            Cors = new CorsConfig().Init(),
            Email = new EmailConfig().Init().Validate()
        };

        _loaded = true;
        return _config;
    }

    /// <summary>
    /// Gets the current loaded configuration (throws if not loaded)
    /// </summary>
    public static AppConfig Current => _config ?? throw new InvalidOperationException("Configuration not loaded. Call ConfigLoader.Load() first.");

    /// <summary>
    /// Reloads configuration (useful for testing)
    /// </summary>
    public static AppConfig Reload(string? envFilePath = null)
    {
        _loaded = false;
        _config = null;
        return Load(envFilePath);
    }

    private static void LoadEnvFile(string? envFilePath)
    {
        // Try custom path first
        if (!string.IsNullOrWhiteSpace(envFilePath) && File.Exists(envFilePath))
        {
            Env.Load(envFilePath);
            return;
        }

        // TODO: needs review
        // Search for .env in current and parent directories (up to 5 levels)
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        for (var i = 0; i < 5 && dir != null; i++)
        {
            var envPath = Path.Combine(dir.FullName, ".env");
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                return;
            }
            dir = dir.Parent;
        }

        // .env file is optional, environment variables might be set directly
    }
}

