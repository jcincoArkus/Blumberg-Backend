using Adapters.Config;
using Adapters.Database;
using Adapters.Jwt;
using Shared.Abstractions;
using Adapters.OpenAPI.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        if (!string.IsNullOrEmpty(url))
        {
            builder.WebHost.UseUrls(url);
        }

        ConfigureServices(builder.Services, config);

        var app = builder.Build();

        ConfigureMiddleware(app);

        return new ApiServer(app, config);
    }

    private static void ConfigureServices(IServiceCollection services, AppConfig config)
    {
        // Controllers and API Explorer
        var mvcBuilder = services.AddControllers();
        foreach (var assembly in ModulesSetup.GetControllerAssemblies())
        {
            mvcBuilder.AddApplicationPart(assembly);
        }
        services.AddEndpointsApiExplorer();

        // Swagger with JWT security
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
            options.OperationFilter<AuthorizeCheckOperationFilter>();
        });

        // Tenant context (JWT orgId claim or X-Organization-Id header for dev)
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, TenantContext>();

        // Database setup
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(config.Database.GetConnectionString()));

        // JWT Authentication
        services.AddJwtAuthentication(config);

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Application modules
        services.AddApplicationModules();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blumberg API v1");
            });
        }
        else
        {
            // Always enable Swagger for OpenAPI generation
            app.UseSwagger();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }

    public void Run() => _app.Run();

    public Task RunAsync(CancellationToken cancellationToken = default)
        => _app.RunAsync(cancellationToken);

    public Task StartAsync(CancellationToken cancellationToken = default)
        => _app.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken = default)
        => _app.StopAsync(cancellationToken);
}

