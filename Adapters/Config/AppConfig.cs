namespace Adapters.Config;

/// <summary>
/// Main application configuration container
/// </summary>
/// <remarks>
/// Aggregates all configuration sections (Database, JWT, Application, Log, Telemetry).
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
/// var logLevel = config.Log.Level;
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

    /// <summary>
    /// Logger configuration section
    /// </summary>
    public LogConfig Log { get; internal set; } = new();

    /// <summary>
    /// Telemetry configuration section
    /// </summary>
    public TelemetryConfig Telemetry { get; internal set; } = new();

    /// <summary>
    /// CORS configuration (orígenes permitidos para el frontend).
    /// </summary>
    public CorsConfig Cors { get; internal set; } = new();

    /// <summary>
    /// Alert email notification configuration (optional; None = no-op sender).
    /// </summary>
    public EmailConfig Email { get; internal set; } = new();
}
