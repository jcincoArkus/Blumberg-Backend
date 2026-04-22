namespace Adapters.Config;

/// <summary>
/// Telemetry configuration section for OpenTelemetry
/// </summary>
/// <remarks>
/// Configures distributed tracing with OpenTelemetry.
/// Supports multiple exporters: Console, OTLP (Jaeger, Datadog, etc.)
/// </remarks>
public class TelemetryConfig
{
    /// <summary>
    /// Whether telemetry is enabled
    /// </summary>
    /// <remarks>
    /// Default: true
    /// Environment variable: OTEL_ENABLED
    /// </remarks>
    public bool Enabled { get; private set; } = true;

    /// <summary>
    /// Service name for telemetry
    /// </summary>
    /// <remarks>
    /// Default: Blumberg.API
    /// Environment variable: OTEL_SERVICE_NAME
    /// </remarks>
    public string ServiceName { get; private set; } = "Blumberg.API";

    /// <summary>
    /// Service version for telemetry
    /// </summary>
    /// <remarks>
    /// Default: 1.0.0
    /// Environment variable: OTEL_SERVICE_VERSION
    /// </remarks>
    public string ServiceVersion { get; private set; } = "1.0.0";

    /// <summary>
    /// Exporter type (console, otlp, datadog, or multiple separated by comma)
    /// </summary>
    /// <remarks>
    /// Default: console
    /// Environment variable: OTEL_EXPORTER
    /// Options: console, otlp, datadog, console,otlp
    /// </remarks>
    public string Exporter { get; private set; } = "console";

    /// <summary>
    /// OTLP endpoint for exporting traces
    /// </summary>
    /// <remarks>
    /// Default: http://localhost:4317
    /// Environment variable: OTEL_EXPORTER_OTLP_ENDPOINT
    /// Examples:
    /// - Jaeger: http://localhost:4317
    /// - Datadog: http://localhost:4318
    /// </remarks>
    public string OtlpEndpoint { get; private set; } = "http://localhost:4317";

    /// <summary>
    /// OTLP protocol (grpc or http/protobuf)
    /// </summary>
    /// <remarks>
    /// Default: grpc
    /// Environment variable: OTEL_EXPORTER_OTLP_PROTOCOL
    /// Options: grpc, http/protobuf
    /// </remarks>
    public string OtlpProtocol { get; private set; } = "grpc";

    /// <summary>
    /// OTLP headers for authentication (e.g., Datadog API key)
    /// </summary>
    /// <remarks>
    /// Default: empty
    /// Environment variable: OTEL_EXPORTER_OTLP_HEADERS
    /// Format: key1=value1,key2=value2
    /// Example for Datadog: dd-api-key=YOUR_API_KEY
    /// </remarks>
    public string OtlpHeaders { get; private set; } = string.Empty;

    /// <summary>
    /// Datadog API key (simplified configuration)
    /// </summary>
    /// <remarks>
    /// Default: empty
    /// Environment variable: OTEL_DATADOG_API_KEY
    /// When using OTEL_EXPORTER=datadog, this is required
    /// </remarks>
    public string DatadogApiKey { get; private set; } = string.Empty;

    /// <summary>
    /// Datadog site (US, EU, etc.)
    /// </summary>
    /// <remarks>
    /// Default: datadoghq.com
    /// Environment variable: OTEL_DATADOG_SITE
    /// Options: datadoghq.com, datadoghq.eu, us3.datadoghq.com, us5.datadoghq.com
    /// </remarks>
    public string DatadogSite { get; private set; } = "datadoghq.com";

    /// <summary>
    /// Whether to enable Entity Framework Core instrumentation
    /// </summary>
    /// <remarks>
    /// Default: true
    /// Environment variable: OTEL_INSTRUMENTATION_EF_ENABLED
    /// </remarks>
    public bool InstrumentEntityFramework { get; private set; } = true;

    /// <summary>
    /// Whether to enable HTTP client instrumentation
    /// </summary>
    /// <remarks>
    /// Default: true
    /// Environment variable: OTEL_INSTRUMENTATION_HTTP_ENABLED
    /// </remarks>
    public bool InstrumentHttpClient { get; private set; } = true;

    /// <summary>
    /// Whether to enable ASP.NET Core instrumentation
    /// </summary>
    /// <remarks>
    /// Default: true
    /// Environment variable: OTEL_INSTRUMENTATION_ASPNETCORE_ENABLED
    /// </remarks>
    public bool InstrumentAspNetCore { get; private set; } = true;

    /// <summary>
    /// Initializes the configuration from environment variables
    /// </summary>
    /// <returns>This instance for chaining</returns>
    public TelemetryConfig Init()
    {
        Enabled = EnvHelper.GetEnvBool("OTEL_ENABLED", Enabled);
        ServiceName = EnvHelper.GetEnv("OTEL_SERVICE_NAME", ServiceName);
        ServiceVersion = EnvHelper.GetEnv("OTEL_SERVICE_VERSION", ServiceVersion);
        Exporter = EnvHelper.GetEnv("OTEL_EXPORTER", Exporter);
        OtlpEndpoint = EnvHelper.GetEnv("OTEL_EXPORTER_OTLP_ENDPOINT", OtlpEndpoint);
        OtlpProtocol = EnvHelper.GetEnv("OTEL_EXPORTER_OTLP_PROTOCOL", OtlpProtocol);
        OtlpHeaders = EnvHelper.GetEnv("OTEL_EXPORTER_OTLP_HEADERS", OtlpHeaders);
        DatadogApiKey = EnvHelper.GetEnv("OTEL_DATADOG_API_KEY", DatadogApiKey);
        DatadogSite = EnvHelper.GetEnv("OTEL_DATADOG_SITE", DatadogSite);
        InstrumentEntityFramework = EnvHelper.GetEnvBool("OTEL_INSTRUMENTATION_EF_ENABLED", InstrumentEntityFramework);
        InstrumentHttpClient = EnvHelper.GetEnvBool("OTEL_INSTRUMENTATION_HTTP_ENABLED", InstrumentHttpClient);
        InstrumentAspNetCore = EnvHelper.GetEnvBool("OTEL_INSTRUMENTATION_ASPNETCORE_ENABLED", InstrumentAspNetCore);
        return this;
    }

    /// <summary>
    /// Validates the configuration
    /// </summary>
    /// <returns>This instance for chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid</exception>
    public TelemetryConfig Validate()
    {
        if (!Enabled) return this;
        
        if (string.IsNullOrWhiteSpace(ServiceName))
            throw new InvalidOperationException("OTEL_SERVICE_NAME is required when telemetry is enabled");

        if (string.IsNullOrWhiteSpace(Exporter))
            throw new InvalidOperationException("OTEL_EXPORTER is required when telemetry is enabled");

        var validExporters = new[] { "console", "otlp", "datadog" };
        var exporters = Exporter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var exporter in exporters)
        {
            if (!validExporters.Contains(exporter.ToLowerInvariant()))
                throw new InvalidOperationException($"Invalid OTEL_EXPORTER: {exporter}. Valid options: console, otlp, datadog");
        }

        // Validate Datadog configuration
        if (exporters.Contains("datadog", StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(DatadogApiKey))
                throw new InvalidOperationException("OTEL_DATADOG_API_KEY is required when using datadog exporter");
        }

        if (!exporters.Contains("otlp", StringComparer.OrdinalIgnoreCase)) return this;
        
        if (string.IsNullOrWhiteSpace(OtlpEndpoint))
            throw new InvalidOperationException("OTEL_EXPORTER_OTLP_ENDPOINT is required when using OTLP exporter");

        return !Uri.TryCreate(OtlpEndpoint, UriKind.Absolute, out _) ? throw new InvalidOperationException($"Invalid OTEL_EXPORTER_OTLP_ENDPOINT: {OtlpEndpoint}") : this;
    }
}

