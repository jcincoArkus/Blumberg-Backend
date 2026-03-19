using System.Text.Json;
using Adapters.Config;
using Adapters.Database;
using Adapters.Database.Seeders;
using Adapters.Email;
using Adapters.Jwt;
using Adapters.Logger;
using Adapters.Telemetry;
using Adapters.Permissions;
using Shared.Abstractions;
using Adapters.OpenAPI.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Modules;

namespace Adapters.Server;

public class ApiServer
{
    private readonly WebApplication _app;
    private readonly AppConfig _config;

    public WebApplication App => _app;

    private ApiServer(WebApplication app, AppConfig config)
    {
        _app = app;
        _config = config;
    }

    public static ApiServer Create(string[]? args = null, string? url = null)
    {
        var config = ConfigLoader.Load();

        var builder = WebApplication.CreateBuilder(args ?? []);

        builder.AddStructuredLogging(config.Log);
        builder.AddDistributedTracing(config.Telemetry);

        if (!string.IsNullOrEmpty(url))
            builder.WebHost.UseUrls(url);

        ConfigureServices(builder.Services, config, builder.Environment);

        var app = builder.Build();

        ConfigureMiddleware(app);

        return new ApiServer(app, config);
    }

    private static void ConfigureServices(IServiceCollection services, AppConfig config, IWebHostEnvironment env)
    {
        services.AddSingleton(config);

        // Controllers and API Explorer (camelCase JSON so frontend receives equipmentName, sensorSerial, etc.)
        var mvcBuilder = services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            });
        foreach (var assembly in ModulesSetup.GetControllerAssemblies())
        {
            mvcBuilder.AddApplicationPart(assembly);
        }
        services.AddEndpointsApiExplorer();

        // Swagger with JWT and API key security
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Enter the JWT token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "X-Api-Key",
                Type = SecuritySchemeType.ApiKey,
                Description = "API key for ingestion (machine-to-machine). Use header X-Api-Key."
            });
            options.OperationFilter<AuthorizeCheckOperationFilter>();
        });

        // Tenant context (JWT orgId claim or X-Organization-Id header for dev)
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, TenantContext>();

        // Database setup
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(config.Database.GetConnectionString()));

        // JWT + API key authentication (API key for ingestion / M2M)
        services.AddJwtAuthentication(config).AddApiKeyAuthentication();

        // Alert email notifications (NoOp or SMTP based on config)
        services.AddAlertEmailNotifications(config);

        // Casbin Authorization
        services.AddCasbinAuthorization();

        // CORS: explicit allowlist in all environments via CORS_ORIGINS.
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(config.Cors.AllowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Application modules
        services.AddApplicationModules();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        // Skip database initialization if SKIP_DB_INIT environment variable is set
        var skipDbInit = Environment.GetEnvironmentVariable("SKIP_DB_INIT");
        if (skipDbInit != "true")
        {
            // Initialize permission system (migrations, policies, role metadata)
            InitializePermissionSystemAsync(app.Services).GetAwaiter().GetResult();

            // Dev only: run seeders so DB has org, sites, equipment, sensors, readings, admins (idempotent)
            if (app.Environment.IsDevelopment())
            {
                RunSeedersIfDevelopmentAsync(app.Services).GetAwaiter().GetResult();
            }
        }

        // Request logging (must be early in pipeline)
        app.UseStructuredRequestLogging();

        // Swagger and Swagger UI first so /swagger and /swagger/v1/swagger.json are served
        // before auth (avoids 403 Forbidden when opening Swagger UI unauthenticated)
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            // NOTE:
            // - This enables Swagger UI in all environments (including ECS).
            // - Access via /swagger behind your ALB/CloudFront, e.g. https://<tu-dominio>/swagger
            // - If you ever need to restrict it, gate this with an env var like ENABLE_SWAGGER_UI.
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blumberg API v1");
            c.RoutePrefix = "swagger";
        });

        // Routing must run before CORS so the CORS middleware can apply the policy correctly (required for preflight and CORS headers on responses)
        app.UseRouting();

        // CORS after Routing, antes de Auth — así el preflight OPTIONS y todas las respuestas incluyen Access-Control-Allow-Origin.
        app.UseCors();

        // Preflight OPTIONS: 204 and CORS headers using configured allowlist.
        app.Use(async (context, next) =>
        {
            if (context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                var origin = context.Request.Headers.Origin.FirstOrDefault();
                var config = context.RequestServices.GetRequiredService<AppConfig>();
                var allowed = config.Cors.AllowedOrigins;
                if (!string.IsNullOrEmpty(origin) && allowed.Contains(origin, StringComparer.OrdinalIgnoreCase))
                    context.Response.Headers["Access-Control-Allow-Origin"] = origin;
                context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, PATCH, DELETE, OPTIONS";
                context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Api-Key, X-Organization-Id";
                context.Response.Headers["Access-Control-Max-Age"] = "86400";
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }
            await next(context);
        });

        app.UseAuthentication();
        app.UseAuthorization();

        // Health check público para el ALB (no requiere autenticación)
        app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
           .AllowAnonymous();
        app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy" }))
           .AllowAnonymous();

        app.MapControllers();
    }

    /// <summary>
    /// Initializes the permission system automatically on startup
    /// </summary>
    private static async Task InitializePermissionSystemAsync(IServiceProvider serviceProvider)
    {
        await serviceProvider.InitializePermissionSystemAsync();
    }

    /// <summary>
    /// Runs all seeders in Development using design-time context (no tenant).
    /// Seeders are idempotent and skip when data already exists.
    /// Each seeder logs whether it created data or skipped (already present).
    /// </summary>
    private static async Task RunSeedersIfDevelopmentAsync(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ApiServer");
        logger.LogInformation("Dev seed: running (idempotent — seeders log created/skipped below)");

        await using var context = ApplicationDbContextFactory.Create();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        await SeederRunner.RunSeedersAsync(context, loggerFactory);

        logger.LogInformation("Dev seed: finished");
    }

    public void Run() => _app.Run();

    public Task RunAsync(CancellationToken cancellationToken = default)
        => _app.RunAsync(cancellationToken);

    public Task StartAsync(CancellationToken cancellationToken = default)
        => _app.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken = default)
        => _app.StopAsync(cancellationToken);
}

