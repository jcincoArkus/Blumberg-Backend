namespace Adapters.Config;

/// <summary>
/// Helper class for reading environment variables
/// </summary>
internal static class EnvHelper
{
    /// <summary>
    /// Gets a required environment variable (throws if not found)
    /// </summary>
    /// <param name="key">Environment variable name</param>
    /// <returns>Environment variable value</returns>
    /// <exception cref="InvalidOperationException">Thrown when variable is not set</exception>
    public static string GetEnvRequired(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? throw new InvalidOperationException($"Required environment variable '{key}' is not set") : value;
    }

    /// <summary>
    /// Gets an environment variable with a default value
    /// </summary>
    /// <param name="key">Environment variable name</param>
    /// <param name="defaultValue">Default value if not found</param>
    /// <returns>Environment variable value or default</returns>
    public static string GetEnv(string key, string defaultValue = "")
    {
        return Environment.GetEnvironmentVariable(key) ?? defaultValue;
    }

    /// <summary>
    /// Gets an integer environment variable with a default value
    /// </summary>
    /// <param name="key">Environment variable name</param>
    /// <param name="defaultValue">Default value if not found or invalid</param>
    /// <returns>Environment variable value as integer or default</returns>
    public static int GetEnvInt(string key, int defaultValue = 0)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Gets a boolean environment variable with a default value
    /// </summary>
    /// <param name="key">Environment variable name</param>
    /// <param name="defaultValue">Default value if not found or invalid</param>
    /// <returns>Environment variable value as boolean or default</returns>
    public static bool GetEnvBool(string key, bool defaultValue = false)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }
}

