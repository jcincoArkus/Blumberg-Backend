namespace Adapters.Config;

/// <summary>
/// Logger configuration section
/// </summary>
/// <remarks>
/// Configures structured logging with Serilog.
/// Supports both JSON format (production) and colored console format (development).
/// Similar to Zap logger configuration in Go.
/// </remarks>
public class LogConfig
{
    /// <summary>
    /// Log level (Debug, Information, Warning, Error, Fatal)
    /// </summary>
    /// <remarks>
    /// Default: Information
    /// Environment variable: LOG_LEVEL
    /// </remarks>
    public string Level { get; private set; } = "Information";

    /// <summary>
    /// Whether to use JSON format for logs
    /// </summary>
    /// <remarks>
    /// true: JSON format (production, machine-readable)
    /// false: Console format with colors (development, human-readable)
    /// Default: false
    /// Environment variable: LOG_FORMAT_JSON
    /// </remarks>
    public bool FormatJson { get; private set; } = false;

    /// <summary>
    /// Whether to write logs to file
    /// </summary>
    /// <remarks>
    /// Default: true
    /// Environment variable: LOG_WRITE_FILE
    /// </remarks>
    public bool WriteToFile { get; private set; } = true;

    /// <summary>
    /// Log file path (when WriteToFile is true)
    /// </summary>
    /// <remarks>
    /// Default: logs/blumberg-.json
    /// Environment variable: LOG_FILE_PATH
    /// </remarks>
    public string FilePath { get; private set; } = "logs/blumberg-.json";

    /// <summary>
    /// Number of days to retain log files
    /// </summary>
    /// <remarks>
    /// Default: 30
    /// Environment variable: LOG_RETENTION_DAYS
    /// </remarks>
    public int RetentionDays { get; private set; } = 30;

    /// <summary>
    /// Initializes the configuration from environment variables
    /// </summary>
    /// <returns>This instance for chaining</returns>
    public LogConfig Init()
    {
        Level = EnvHelper.GetEnv("LOG_LEVEL", Level);
        FormatJson = EnvHelper.GetEnvBool("LOG_FORMAT_JSON", FormatJson);
        WriteToFile = EnvHelper.GetEnvBool("LOG_WRITE_FILE", WriteToFile);
        FilePath = EnvHelper.GetEnv("LOG_FILE_PATH", FilePath);
        RetentionDays = EnvHelper.GetEnvInt("LOG_RETENTION_DAYS", RetentionDays);
        return this;
    }

    /// <summary>
    /// Validates the configuration
    /// </summary>
    /// <returns>This instance for chaining</returns>
    /// <exception cref="InvalidOperationException">When configuration is invalid</exception>
    public LogConfig Validate()
    {
        var validLevels = new[] { "Debug", "Information", "Warning", "Error", "Fatal" };
        if (!validLevels.Contains(Level, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Invalid log level: {Level}. Valid values: {string.Join(", ", validLevels)}");
        }

        if (WriteToFile && string.IsNullOrWhiteSpace(FilePath))
        {
            throw new InvalidOperationException("LOG_FILE_PATH is required when LOG_WRITE_FILE is true");
        }

        return RetentionDays < 1 ? throw new InvalidOperationException("LOG_RETENTION_DAYS must be at least 1") : this;
    }
}

