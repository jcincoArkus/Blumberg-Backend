using System.CommandLine;
using CLI.Generators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Adapters.Permissions;

namespace CLI.Commands;

/// <summary>
/// Permission system commands for code generation
/// </summary>
public static class PermissionCommands
{
    /// <summary>
    /// Generates type-safe permission code from Casbin policy CSV
    /// </summary>
    public static Command GeneratePermissions(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("PermissionGenerate");
        var command = new Command("permission:generate", "Generate type-safe permission code from Casbin policy CSV");

        command.SetHandler(async () =>
        {
            try
            {
                logger.LogInformation("Starting permission code generation");

                var generator = new PermissionGenerator(logger);
                await generator.GenerateAsync();

                logger.LogInformation("Permission code generation completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate permission code");
                Environment.Exit(1);
            }
        });

        return command;
    }

    /// <summary>
    /// Initializes the permission system (migrations, policies, role metadata)
    /// </summary>
    public static Command InitializePermissions(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("PermissionInit");
        var command = new Command("permission:init", "Initialize permission system (migrations, policies, role metadata)");

        command.SetHandler(async () =>
        {
            try
            {
                logger.LogInformation("Initializing permission system");
                await serviceProvider.InitializePermissionSystemAsync();
                logger.LogInformation("Permission system initialized successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize permission system");
                Environment.Exit(1);
            }
        });

        return command;
    }

    /// <summary>
    /// Verifies that the permission system is properly initialized
    /// </summary>
    public static Command VerifyPermissions(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("PermissionVerify");
        var command = new Command("permission:verify", "Verify permission system is properly initialized");

        command.SetHandler(async () =>
        {
            try
            {
                logger.LogInformation("Verifying permission system");
                var initializer = serviceProvider.GetRequiredService<PermissionInitializer>();
                var isInitialized = await initializer.VerifyInitializationAsync();

                if (isInitialized)
                {
                    logger.LogInformation("✅ Permission system is properly initialized");
                }
                else
                {
                    logger.LogWarning("⚠️ Permission system may not be fully initialized");
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to verify permission system");
                Environment.Exit(1);
            }
        });

        return command;
    }
}

