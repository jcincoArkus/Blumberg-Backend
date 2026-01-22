namespace Adapters.Config;

/// <summary>
/// Main application configuration container
/// </summary>
/// <remarks>
/// Aggregates all configuration sections (Database, JWT, Application).
/// Each section is self-initializing and self-validating.
/// Load using ConfigLoader.Load() or via dependency injection with AddAppConfiguration().
/// </remarks>
/// <example>
/// <code>
/// // Load configuration
/// var config = ConfigLoader.Load();
///
/// // Access sections
/// var connectionString = config.Database.GetConnectionString();
/// var jwtSecret = config.Jwt.SecretKey;
/// var isDev = config.Application.IsDevelopment;
///
/// // Or via DI
/// services.AddAppConfiguration();
///
/// // Inject in controllers/services
/// public MyService(AppConfig config) { }
/// </code>
/// </example>
public class AppConfig
{
    /// <summary>
    /// Database configuration section
    /// </summary>
    public DatabaseConfig Database { get; internal set; } = new();

    /// <summary>
    /// JWT authentication configuration section
    /// </summary>
    public JwtConfig Jwt { get; internal set; } = new();

    /// <summary>
    /// Application-level configuration section
    /// </summary>
    public ApplicationConfig Application { get; internal set; } = new();
}
