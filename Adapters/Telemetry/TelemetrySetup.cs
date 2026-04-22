using Adapters.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Exporter;
using OpenTelemetry.Extensions.Propagators;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Adapters.Telemetry;

/// <summary>
/// Extension methods for setting up OpenTelemetry distributed tracing
/// </summary>
/// <remarks>
/// Provides distributed tracing with OpenTelemetry.
/// Supports automatic instrumentation of HTTP, ASP.NET Core, and Entity Framework Core.
/// Supports multiple exporters: Console, OTLP (Jaeger, Datadog, etc.)
/// </remarks>
public static class TelemetrySetup
{
    /// <summary>
    /// Configures OpenTelemetry tracing
    /// </summary>
    /// <param name="builder">Web application builder</param>
    /// <param name="config">Telemetry configuration</param>
    /// <returns>Web application builder for chaining</returns>
    public static WebApplicationBuilder AddDistributedTracing(
        this WebApplicationBuilder builder,
        TelemetryConfig config)
    {
        // Register TelemetryConfig in DI container for SpanInterceptor
        builder.Services.AddSingleton(config);

        if (!config.Enabled)
        {
            return builder;
        }

        // Configure context propagation (supports W3C, B3, and Datadog headers)
        Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(new TextMapPropagator[]
        {
            new TraceContextPropagator(),                      // W3C Trace Context (traceparent, tracestate)
            new BaggagePropagator(),                           // W3C Baggage
            new OpenTelemetry.Extensions.Propagators.B3Propagator(),  // Zipkin B3 (X-B3-TraceId, X-B3-SpanId, etc.)
            // Note: Datadog uses W3C Trace Context by default now
            // Legacy Datadog headers (x-datadog-trace-id) are also supported via B3
        }));

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: config.ServiceName,
                    serviceVersion: config.ServiceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = builder.Environment.EnvironmentName,
                    ["host.name"] = Environment.MachineName
                }))
            .WithTracing(tracing =>
            {
                // Add sources
                tracing.AddSource(ActivitySourceProvider.SourceName);

                // Automatic instrumentation
                if (config.InstrumentAspNetCore)
                {
                    tracing.AddAspNetCoreInstrumentation(options =>
                    {
                        // Enrich spans with additional information
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            activity.SetTag("http.request.content_length", request.ContentLength);
                            activity.SetTag("http.request.content_type", request.ContentType);
                        };

                        options.EnrichWithHttpResponse = (activity, response) =>
                        {
                            activity.SetTag("http.response.content_length", response.ContentLength);
                            activity.SetTag("http.response.content_type", response.ContentType);
                        };

                        // Filter out health check endpoints
                        options.Filter = (httpContext) =>
                        {
                            return !httpContext.Request.Path.StartsWithSegments("/health");
                        };
                    });
                }

                if (config.InstrumentHttpClient)
                {
                    tracing.AddHttpClientInstrumentation(options =>
                    {
                        // Enrich spans with additional information
                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            activity.SetTag("http.request.method", request.Method.Method);
                            activity.SetTag("http.request.uri", request.RequestUri?.ToString());
                        };

                        options.EnrichWithHttpResponseMessage = (activity, response) =>
                        {
                            activity.SetTag("http.response.status_code", (int)response.StatusCode);
                        };
                    });
                }

                if (config.InstrumentEntityFramework)
                {
                    tracing.AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        // Enable detailed query information
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                    });
                }

                // Configure exporters
                ConfigureExporters(tracing, config);
            });

        return builder;
    }

    private static void ConfigureExporters(TracerProviderBuilder tracing, TelemetryConfig config)
    {
        var exporters = config.Exporter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var exporter in exporters)
        {
            switch (exporter.ToLowerInvariant())
            {
                case "console":
                    tracing.AddConsoleExporter(options =>
                    {
                        options.Targets = ConsoleExporterOutputTargets.Console;
                    });
                    break;

                case "otlp":
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(config.OtlpEndpoint);
                        options.Protocol = config.OtlpProtocol.ToLowerInvariant() == "grpc"
                            ? OtlpExportProtocol.Grpc
                            : OtlpExportProtocol.HttpProtobuf;

                        // Add headers for authentication (e.g., Datadog API key)
                        if (!string.IsNullOrWhiteSpace(config.OtlpHeaders))
                        {
                            var headers = config.OtlpHeaders.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            foreach (var header in headers)
                            {
                                var parts = header.Split('=', 2);
                                if (parts.Length == 2)
                                {
                                    options.Headers += $"{parts[0]}={parts[1]},";
                                }
                            }
                            // Remove trailing comma
                            if (options.Headers?.EndsWith(',') == true)
                            {
                                options.Headers = options.Headers.TrimEnd(',');
                            }
                        }
                    });
                    break;

                case "datadog":
                    // Datadog exporter using OTLP with simplified configuration
                    tracing.AddOtlpExporter(options =>
                    {
                        // Datadog OTLP endpoint (agentless)
                        var datadogEndpoint = config.DatadogSite switch
                        {
                            "datadoghq.eu" => "https://api.datadoghq.eu:443",
                            "us3.datadoghq.com" => "https://api.us3.datadoghq.com:443",
                            "us5.datadoghq.com" => "https://api.us5.datadoghq.com:443",
                            "ddog-gov.com" => "https://api.ddog-gov.com:443",
                            _ => "https://api.datadoghq.com:443" // Default to US
                        };

                        options.Endpoint = new Uri(datadogEndpoint);
                        options.Protocol = OtlpExportProtocol.HttpProtobuf; // Datadog requires HTTP
                        options.Headers = $"dd-api-key={config.DatadogApiKey}";
                    });
                    break;
            }
        }
    }
}

