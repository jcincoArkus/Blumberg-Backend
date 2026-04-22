using System.CommandLine;
using System.Net.Sockets;
using Adapters.Server;
using Microsoft.Extensions.Logging;

namespace CLI.Commands;

public static class OpenApiCommands
{
    public static Command GenerateOpenApi(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("OpenApiGenerate");

        var outputOption = new Option<string>(
            name: "--output",
            description: "Output file path for the YAML",
            getDefaultValue: () => "Adapters/OpenAPI/openapi.yaml");

        var command = new Command("openapi:generate", "Generate OpenAPI YAML specification")
        {
            outputOption
        };

        command.SetHandler(async (output) =>
        {
            await GenerateOpenApiYaml(output, logger);
        }, outputOption);

        return command;
    }

    private static int GetRandomAvailablePort()
    {
        var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static async Task GenerateOpenApiYaml(string outputPath, ILogger logger)
    {
        logger.LogInformation("Generating OpenAPI specification");

        var port = GetRandomAvailablePort();
        var baseUrl = $"http://localhost:{port}";

        logger.LogInformation("Starting temporary server on port {Port}", port);

        // Set environment variable to skip database initialization for OpenAPI generation
        Environment.SetEnvironmentVariable("SKIP_DB_INIT", "true");

        try
        {
            var server = ApiServer.Create(url: baseUrl);
            await server.StartAsync();

            await Task.Delay(500);

            using var httpClient = new HttpClient();
            var yamlContent = await httpClient.GetStringAsync($"{baseUrl}/swagger/v1/swagger.yaml");

            var resolvedPath = Path.IsPathRooted(outputPath)
                ? outputPath
                : Path.Combine(Directory.GetCurrentDirectory(), outputPath);

            var directory = Path.GetDirectoryName(resolvedPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(resolvedPath, yamlContent);

            logger.LogInformation("OpenAPI spec saved to: {Path}", resolvedPath);

            await server.StopAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate OpenAPI specification");
        }
    }
}

