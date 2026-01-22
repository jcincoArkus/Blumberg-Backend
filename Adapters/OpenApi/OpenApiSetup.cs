using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace Adapters.OpenApi;

/// <summary>
/// Extension methods for setting up OpenAPI/Swagger
/// </summary>
public static class OpenApiSetup
{
    /// <summary>
    /// Adds Swagger/OpenAPI documentation to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="title">API title</param>
    /// <param name="version">API version</param>
    /// <param name="description">API description (optional)</param>
    /// <param name="xmlDocAssemblies">Assemblies to include XML documentation from (optional)</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddOpenApiDocumentation(
        this IServiceCollection services,
        string title = "Blumberg API",
        string version = "v1",
        string? description = null,
        params Assembly[]? xmlDocAssemblies)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // API Info
            options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = title,
                Version = version,
                Description = description ?? $"{title} - RESTful API Documentation"
            });

            // JWT Bearer Authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Enter the JWT token in the format: Bearer {your token}",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });

            // Add operation filter to automatically add security requirements
            options.OperationFilter<AuthorizeCheckOperationFilter>();

            // Enable XML comments from specified assemblies
            if (xmlDocAssemblies?.Length > 0)
            {
                foreach (var assembly in xmlDocAssemblies)
                {
                    var xmlFile = $"{assembly.GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (File.Exists(xmlPath))
                    {
                        options.IncludeXmlComments(xmlPath);
                    }
                }
            }
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger UI middleware
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="routePrefix">Route prefix for Swagger UI (default: "swagger")</param>
    /// <param name="documentTitle">Document title in browser tab</param>
    /// <returns>Application builder for chaining</returns>
    public static IApplicationBuilder UseOpenApiDocumentation(
        this IApplicationBuilder app,
        string routePrefix = "swagger",
        string documentTitle = "Blumberg API Documentation")
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", documentTitle);
            options.RoutePrefix = routePrefix;
            options.DocumentTitle = documentTitle;

            // Enable deep linking
            options.EnableDeepLinking();

            // Display request duration
            options.DisplayRequestDuration();

            // Enable filter
            options.EnableFilter();

            // Enable validator
            options.EnableValidator();
        });

        return app;
    }

    /// <summary>
    /// Exports the OpenAPI specification to a YAML file
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="outputPath">Output file path (default: "openapi.yml")</param>
    /// <param name="version">API version to export (default: "v1")</param>
    /// <remarks>
    /// This method should be called after UseOpenApiDocumentation() and before app.Run().
    /// It will generate the YAML file and then exit the application.
    /// Typically used with a command-line flag like: dotnet run --export-openapi
    /// </remarks>
    /// <example>
    /// <code>
    /// if (args.Contains("--export-openapi"))
    /// {
    ///     app.ExportOpenApiSpec("docs/openapi.yml");
    ///     return;
    /// }
    /// </code>
    /// </example>
    public static void ExportOpenApiSpec(
        this IApplicationBuilder app,
        string outputPath = "openapi.yml",
        string version = "v1")
    {
        var swaggerProvider = app.ApplicationServices.GetRequiredService<Swashbuckle.AspNetCore.Swagger.ISwaggerProvider>();
        var openApiDocument = swaggerProvider.GetSwagger(version);

        // Ensure directory exists
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Write to YAML file
        using var writer = new StreamWriter(outputPath);
        var yamlWriter = new OpenApiYamlWriter(writer);
        openApiDocument.SerializeAsV3(yamlWriter);

        Console.WriteLine($"✅ OpenAPI spec exported to: {Path.GetFullPath(outputPath)}");
    }
}

