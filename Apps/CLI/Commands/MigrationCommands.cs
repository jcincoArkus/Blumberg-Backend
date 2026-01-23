using System.CommandLine;
using System.Reflection;
using Adapters.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace CLI.Commands;

/// <summary>
/// Migration commands for database operations
/// </summary>
public static class MigrationCommands
{
    /// <summary>
    /// Creates the migration:generate command
    /// </summary>
    public static Command MigrationGenerate()
    {
        var command = new Command("migration:generate", "Generate a new migration from schema changes");
        var nameArg = new Argument<string>("name", "Name of the migration");
        command.AddArgument(nameArg);

        command.SetHandler((string name) =>
        {
            Console.WriteLine($"[INFO] Generating migration '{name}'...");

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
                var rootNamespace = "Adapters.Database";

                var migration = scaffolder.ScaffoldMigration(name, rootNamespace);

                // Check if migration is empty
                if (IsMigrationEmpty(migration))
                {
                    Console.WriteLine("[WARNING] No schema changes detected.");
                    Console.WriteLine("[INFO] No migration needed - schemas are up to date.");
                    return;
                }

                // Save migration files
                var outputDir = Path.Combine(projectDir, "Migrations");
                scaffolder.Save(projectDir, migration, outputDir);

                Console.WriteLine($"[SUCCESS] Migration '{name}' generated successfully.");
                Console.WriteLine($"  Files created in: {outputDir}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to generate migration: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"  Inner: {ex.InnerException.Message}");
                Environment.Exit(1);
            }
        }, nameArg);

        return command;
    }

    /// <summary>
    /// Creates the migration:up command
    /// </summary>
    public static Command MigrationUp()
    {
        var command = new Command("migration:up", "Apply pending migrations to the database");

        command.SetHandler(async () =>
        {
            Console.WriteLine("[INFO] Applying pending migrations...");

            try
            {
                await using var context = ApplicationDbContextFactory.Create();

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                var pendingList = pendingMigrations.ToList();

                if (pendingList.Count == 0)
                {
                    Console.WriteLine("[INFO] No pending migrations found. Database is up to date.");
                    return;
                }

                Console.WriteLine($"[INFO] Found {pendingList.Count} pending migration(s):");
                foreach (var migration in pendingList)
                {
                    Console.WriteLine($"  - {migration}");
                }

                Console.WriteLine("[INFO] Applying migrations...");
                await context.Database.MigrateAsync();

                Console.WriteLine("[SUCCESS] Migrations applied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to apply migrations: {ex.Message}");
                Environment.Exit(1);
            }
        });

        return command;
    }

    /// <summary>
    /// Creates the migration:down command
    /// </summary>
    public static Command MigrationDown()
    {
        var command = new Command("migration:down", "Rollback the last applied migration");

        command.SetHandler(async () =>
        {
            Console.WriteLine("[INFO] Rolling back last migration...");

            try
            {
                await using var context = ApplicationDbContextFactory.Create();

                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                var appliedList = appliedMigrations.ToList();

                if (appliedList.Count == 0)
                {
                    Console.WriteLine("[INFO] No migrations to rollback.");
                    return;
                }

                // Get the target migration (second to last, or "0" if only one)
                var targetMigration = appliedList.Count > 1
                    ? appliedList[^2]  // Second to last
                    : "0";              // Rollback to empty

                Console.WriteLine($"[INFO] Current migration: {appliedList[^1]}");
                Console.WriteLine($"[INFO] Rolling back to: {(targetMigration == "0" ? "(empty database)" : targetMigration)}");

                // Use the migrator to rollback
                var migrator = context.GetInfrastructure().GetRequiredService<IMigrator>();
                await migrator.MigrateAsync(targetMigration);

                Console.WriteLine("[SUCCESS] Rollback completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to rollback migration: {ex.Message}");
                Environment.Exit(1);
            }
        });

        return command;
    }

    /// <summary>
    /// Creates the migration:status command
    /// </summary>
    public static Command MigrationStatus()
    {
        var command = new Command("migration:status", "Show migration status");

        command.SetHandler(async () =>
        {
            try
            {
                await using var context = ApplicationDbContextFactory.Create();

                var applied = (await context.Database.GetAppliedMigrationsAsync()).ToList();
                var pending = (await context.Database.GetPendingMigrationsAsync()).ToList();

                Console.WriteLine("Migration Status:");
                Console.WriteLine($"  Applied: {applied.Count}");
                Console.WriteLine($"  Pending: {pending.Count}");

                if (applied.Count > 0)
                {
                    Console.WriteLine("\nApplied Migrations:");
                    foreach (var m in applied)
                        Console.WriteLine($"  [x] {m}");
                }

                if (pending.Count > 0)
                {
                    Console.WriteLine("\nPending Migrations:");
                    foreach (var m in pending)
                        Console.WriteLine($"  [ ] {m}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
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

