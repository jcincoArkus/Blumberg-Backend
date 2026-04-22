using System.CommandLine;
using System.Reflection;
using Adapters.Database;
using Adapters.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CLI.Commands;

/// <summary>
/// Migration commands for database operations
/// </summary>
public static class MigrationCommands
{
    /// <summary>
    /// Creates the migration:generate command
    /// </summary>
    public static Command MigrationGenerate(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("MigrationGenerate");
        var command = new Command("migration:generate", "Generate a new migration from schema changes");
        var nameArg = new Argument<string>("name", "Name of the migration");
        command.AddArgument(nameArg);

        command.SetHandler((string name) =>
        {
            logger.LogInformation("Generating migration {MigrationName}", name);

            try
            {
                using var context = ApplicationDbContextFactory.Create();

                // Build design-time services
                var serviceCollection = new ServiceCollection();
                serviceCollection.AddDbContextDesignTimeServices(context);

                // Load and configure provider services (Npgsql)
                var provider = context.GetService<IDatabaseProvider>()!.Name;
                var providerAssembly = Assembly.Load(new AssemblyName(provider));
                var providerServicesAttribute = providerAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>();

                if (providerServicesAttribute != null)
                {
                    var designTimeServicesType = providerAssembly.GetType(providerServicesAttribute.TypeName, throwOnError: true)!;
                    var designTimeServices = (IDesignTimeServices)Activator.CreateInstance(designTimeServicesType)!;
                    designTimeServices.ConfigureDesignTimeServices(serviceCollection);
                }

                serviceCollection.AddEntityFrameworkDesignTimeServices();

                var serviceProvider = serviceCollection.BuildServiceProvider();

                // Scaffold migration
                var scaffolder = serviceProvider.GetRequiredService<IMigrationsScaffolder>();
                var projectDir = GetDatabaseProjectDir();
                const string rootNamespace = "Adapters.Database";

                var migration = scaffolder.ScaffoldMigration(name, rootNamespace);

                // Check if migration is empty
                if (IsMigrationEmpty(migration))
                {
                    // Example: Zap-like syntax with key-value pairs (different types)
                    logger.LogWarnWithProps("no schema changes detected",
                        "migrationName", name,
                        "projectDir", projectDir,
                        "timestamp", DateTime.UtcNow,
                        "isEmpty", true,
                        "changeCount", 0);
                    logger.LogInformation("No migration needed - schemas are up to date");
                    return;
                }

                // Save migration files
                var outputDir = Path.Combine(projectDir, "Migrations");
                scaffolder.Save(projectDir, migration, outputDir);

                logger.LogInformation("Migration {MigrationName} generated successfully. Files created in: {OutputDir}", name, outputDir);
            }
            catch (Exception ex)
            {
                // Example: Zap-like syntax with exception and key-value pairs
                logger.LogErrorWithProps(ex, "failed to generate migration",
                    "migrationName", name,
                    "errorType", ex.GetType().Name);
                Environment.Exit(1);
            }
        }, nameArg);

        return command;
    }

    /// <summary>
    /// Creates the migration:up command
    /// </summary>
    public static Command MigrationUp(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("MigrationUp");
        var command = new Command("migration:up", "Apply pending migrations to the database");

        command.SetHandler(async () =>
        {
            logger.LogInformation("Applying pending migrations");

            try
            {
                await using var context = ApplicationDbContextFactory.Create();

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                var pendingList = pendingMigrations.ToList();

                if (pendingList.Count == 0)
                {
                    logger.LogInformation("No pending migrations found. Database is up to date");
                    return;
                }

                logger.LogInformation("Found {Count} pending migration(s)", pendingList.Count);
                foreach (var migration in pendingList)
                {
                    logger.LogDebug("  - {Migration}", migration);
                }

                logger.LogInformation("Applying migrations");
                await context.Database.MigrateAsync();

                logger.LogInformation("Migrations applied successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to apply migrations");
                Environment.Exit(1);
            }
        });

        return command;
    }

    /// <summary>
    /// Creates the migration:down command
    /// </summary>
    public static Command MigrationDown(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("MigrationDown");
        var command = new Command("migration:down", "Rollback the last applied migration");

        command.SetHandler(async () =>
        {
            logger.LogInformation("Rolling back last migration");

            try
            {
                await using var context = ApplicationDbContextFactory.Create();

                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                var appliedList = appliedMigrations.ToList();

                if (appliedList.Count == 0)
                {
                    logger.LogInformation("No migrations to rollback");
                    return;
                }

                // Get the target migration (second to last, or "0" if only one)
                var targetMigration = appliedList.Count > 1
                    ? appliedList[^2]  // Second to last
                    : "0";              // Rollback to empty

                var targetDisplay = targetMigration == "0" ? "(empty database)" : targetMigration;
                logger.LogInformation("Current migration: {Current}, rolling back to: {Target}", appliedList[^1], targetDisplay);

                // Use the migrator to rollback
                var migrator = context.GetInfrastructure().GetRequiredService<IMigrator>();
                await migrator.MigrateAsync(targetMigration);

                logger.LogInformation("Rollback completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to rollback migration");
                Environment.Exit(1);
            }
        });

        return command;
    }

    /// <summary>
    /// Creates the migration:status command
    /// </summary>
    public static Command MigrationStatus(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("MigrationStatus");
        var command = new Command("migration:status", "Show migration status");

        command.SetHandler(async () =>
        {
            try
            {
                await using var context = ApplicationDbContextFactory.Create();

                var applied = (await context.Database.GetAppliedMigrationsAsync()).ToList();
                var pending = (await context.Database.GetPendingMigrationsAsync()).ToList();

                logger.LogInformation("Migration Status - Applied: {AppliedCount}, Pending: {PendingCount}", applied.Count, pending.Count);

                if (applied.Count > 0)
                {
                    logger.LogInformation("Applied Migrations:");
                    foreach (var m in applied)
                        logger.LogInformation("  [x] {Migration}", m);
                }

                if (pending.Count > 0)
                {
                    logger.LogInformation("Pending Migrations:");
                    foreach (var m in pending)
                        logger.LogInformation("  [ ] {Migration}", m);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get migration status");
                Environment.Exit(1);
            }
        });

        return command;
    }

    /// <summary>
    /// Gets the path to the Database project directory
    /// </summary>
    private static string GetDatabaseProjectDir()
    {
        // Try to find the Adapters/Database directory
        var currentDir = Directory.GetCurrentDirectory();

        // Check if we're in the root project directory
        var databaseDir = Path.Combine(currentDir, "Adapters", "Database");
        if (Directory.Exists(databaseDir))
            return databaseDir;

        // Check parent directories
        var parent = Directory.GetParent(currentDir);
        while (parent != null)
        {
            databaseDir = Path.Combine(parent.FullName, "Adapters", "Database");
            if (Directory.Exists(databaseDir))
                return databaseDir;
            parent = Directory.GetParent(parent.FullName);
        }

        throw new InvalidOperationException("Could not find Adapters/Database project directory");
    }

    /// <summary>
    /// Checks if a scaffolded migration is empty (no operations)
    /// </summary>
    private static bool IsMigrationEmpty(ScaffoldedMigration migration)
    {
        // Check if the migration code contains any migrationBuilder operations
        return !migration.MigrationCode.Contains("migrationBuilder.");
    }
}

