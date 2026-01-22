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
            Application = new ApplicationConfig().Init().Validate()
        };

        _loaded = true;
        return _config;
    }

    /// <summary>
    /// Gets the current loaded configuration (throws if not loaded)
    /// </summary>
    public static AppConfig Current
    {
        get
        {
            if (_config == null)
                throw new InvalidOperationException("Configuration not loaded. Call ConfigLoader.Load() first.");
            return _config;
        }
    }

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
        var currentDir = Directory.GetCurrentDirectory();

        // Try custom path first
        if (!string.IsNullOrWhiteSpace(envFilePath) && File.Exists(envFilePath))
        {
            Env.Load(envFilePath);
            return;
        }

        // Try current directory
        var envPath = Path.Combine(currentDir, ".env");
        if (File.Exists(envPath))
        {
            Env.Load(envPath);
            return;
        }

        // Try parent directory
        var parentDir = Directory.GetParent(currentDir)?.FullName;
        if (parentDir != null)
        {
            var parentEnvPath = Path.Combine(parentDir, ".env");
            if (File.Exists(parentEnvPath))
            {
                Env.Load(parentEnvPath);
                return;
            }
        }

        // .env file is optional, environment variables might be set directly
    }
}

