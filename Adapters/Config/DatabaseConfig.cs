namespace Adapters.Config;

/// <summary>
/// Database configuration settings
/// </summary>
/// <remarks>
/// Configures PostgreSQL database connection parameters.
/// All values can be set via environment variables with DB_ prefix.
/// </remarks>
public class DatabaseConfig
{
    /// <summary>
    /// Database server host
    /// </summary>
    /// <example>localhost, db.example.com, 192.168.1.100</example>
    public string Host { get; private set; } = "localhost";

    /// <summary>
    /// Database server port
    /// </summary>
    /// <example>5432 (default PostgreSQL port)</example>
    public int Port { get; private set; } = 5432;

    /// <summary>
    /// Database name
    /// </summary>
    /// <example>blumberg, blumberg_dev, blumberg_prod</example>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Database username
    /// </summary>
    /// <example>blumberg, postgres, admin</example>
    public string User { get; private set; } = string.Empty;

    /// <summary>
    /// Database password (required)
    /// </summary>
    public string Password { get; private set; } = string.Empty;

    /// <summary>
    /// Initializes configuration from environment variables
    /// </summary>
    public DatabaseConfig Init()
    {
        Host = EnvHelper.GetEnv("DB_HOST", "localhost");
        Port = EnvHelper.GetEnvInt("DB_PORT", 5432);
        Name = EnvHelper.GetEnv("DB_NAME", "blumberg");
        User = EnvHelper.GetEnv("DB_USER", "blumberg");
        Password = EnvHelper.GetEnvRequired("DB_PASSWORD");

        return this;
    }

    /// <summary>
    /// Gets the PostgreSQL connection string
    /// </summary>
    /// <returns>Formatted connection string for Npgsql</returns>
    /// <example>Host=localhost;Port=5432;Database=blumberg;Username=blumberg;Password=secret</example>
    public string GetConnectionString()
    {
        return $"Host={Host};Port={Port};Database={Name};Username={User};Password={Password}";
    }

    /// <summary>
    /// Validates database configuration
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required fields are missing</exception>
    public DatabaseConfig Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Database name (DB_NAME) is required");

        if (string.IsNullOrWhiteSpace(User))
            throw new InvalidOperationException("Database user (DB_USER) is required");

        if (string.IsNullOrWhiteSpace(Password))
            throw new InvalidOperationException("Database password (DB_PASSWORD) is required");

        if (Port <= 0 || Port > 65535)
            throw new InvalidOperationException("Database port must be between 1 and 65535");

        return this;
    }
}

