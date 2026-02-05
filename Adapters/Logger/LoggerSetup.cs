using Adapters.Config;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace Adapters.Logger;

/// <summary>
/// Extension methods for setting up Serilog logging
/// </summary>
/// <remarks>
/// Provides structured logging similar to Zap logger in Go.
/// Supports both JSON format (production) and colored console format (development).
/// </remarks>
public static class LoggerSetup
{
    /// <summary>
    /// Configures Serilog as the logging provider
    /// </summary>
    /// <param name="builder">Web application builder</param>
    /// <param name="config">Logger configuration</param>
    /// <returns>Web application builder for chaining</returns>
    public static WebApplicationBuilder AddStructuredLogging(
        this WebApplicationBuilder builder,
        LogConfig config)
    {
        // Parse log level
        var logLevel = ParseLogLevel(config.Level);

        // Configure Serilog
        builder.Host.UseSerilog((context, services, loggerConfig) =>
        {
            loggerConfig
                .MinimumLevel.Is(logLevel)
                // Override Microsoft logs to reduce noise
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                // Enrich with contextual information
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("Application", "Blumberg")
                .Enrich.WithProperty("Version", GetVersion())
                // Enrich with OpenTelemetry trace context (TraceId, SpanId)
                .Enrich.WithSpan();

            // Console output
            if (config.FormatJson)
            {
                // JSON format for production (machine-readable)
                loggerConfig.WriteTo.Console(new CompactJsonFormatter());
            }
            else
            {
                // Colored console format for development (human-readable)
                // Using expression template for better formatting
                var template = "[{@t:HH:mm:ss} {@l:u3}] {@m}\n{@x}";
                
                loggerConfig.WriteTo.Console(
                    new ExpressionTemplate(
                        template,
                        theme: TemplateTheme.Code));
            }

            // File output (always JSON for structured querying)
            if (config.WriteToFile)
            {
                loggerConfig.WriteTo.File(
                    new CompactJsonFormatter(),
                    config.FilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: config.RetentionDays);
            }
        });

        return builder;
    }

    /// <summary>
    /// Adds Serilog request logging middleware
    /// </summary>
    /// <param name="app">Web application</param>
    /// <returns>Web application for chaining</returns>
    public static WebApplication UseStructuredRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            // Customize the message template
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            // Enrich with additional context
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
                
                // Add tenant context if available
                var orgId = httpContext.User?.FindFirst("orgId")?.Value;
                if (!string.IsNullOrEmpty(orgId))
                {
                    diagnosticContext.Set("OrganizationId", orgId);
                }

                // Add user email if available
                var email = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    diagnosticContext.Set("UserEmail", email);
                }
            };

            // Log level based on status code
            options.GetLevel = (httpContext, elapsed, ex) => ex != null
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode >= 500
                    ? LogEventLevel.Error
                    : httpContext.Response.StatusCode >= 400
                        ? LogEventLevel.Warning
                        : LogEventLevel.Information;
        });

        return app;
    }

    /// <summary>
    /// Parses log level string to Serilog LogEventLevel
    /// </summary>
    private static LogEventLevel ParseLogLevel(string level)
    {
        return level.ToLowerInvariant() switch
        {
            "debug" => LogEventLevel.Debug,
            "information" => LogEventLevel.Information,
            "warning" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }

    /// <summary>
    /// Gets application version from assembly
    /// </summary>
    private static string GetVersion()
    {
        var version = typeof(LoggerSetup).Assembly
            .GetName()
            .Version?
            .ToString() ?? "1.0.0";
        return version;
    }

    /// <summary>
    /// Creates a standalone logger for CLI applications (without ASP.NET)
    /// </summary>
    /// <param name="config">Logger configuration</param>
    /// <returns>Logger instance</returns>
    public static ILogger CreateCliLogger(LogConfig config)
    {
        var logLevel = ParseLogLevel(config.Level);

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "Blumberg.CLI")
            .Enrich.WithProperty("Version", GetVersion())
            // Enrich with OpenTelemetry trace context (TraceId, SpanId)
            .Enrich.WithSpan();

        // Console output
        if (config.FormatJson)
        {
            loggerConfig.WriteTo.Console(new CompactJsonFormatter());
        }
        else
        {
            var template = "[{@t:HH:mm:ss} {@l:u3}] {@m}\n{@x}";
            loggerConfig.WriteTo.Console(
                new ExpressionTemplate(
                    template,
                    theme: TemplateTheme.Code));
        }

        // File output
        if (config.WriteToFile)
        {
            loggerConfig.WriteTo.File(
                new CompactJsonFormatter(),
                config.FilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: config.RetentionDays);
        }

        Log.Logger = loggerConfig.CreateLogger();

        return Log.Logger;
    }
}

