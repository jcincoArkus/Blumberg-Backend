using Adapters.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Adapters.Permissions;

/// <summary>
/// Initializes the permission system on application startup.
/// Handles database migrations, table creation, and policy synchronization.
/// </summary>
public class PermissionInitializer(
    IServiceProvider serviceProvider,
    ILogger<PermissionInitializer> logger)
{
    /// <summary>
    /// Initializes the complete permission system:
    /// 1. Applies pending database migrations
    /// 2. Ensures Casbin tables exist
    /// 3. Syncs policies from CSV to database
    /// 4. Loads role metadata from JSON
    /// </summary>
    public async Task InitializeAsync()
    {
        logger.LogInformation("Starting permission system initialization");

        try
        {
            // Step 1: Apply database migrations
            await ApplyMigrationsAsync();

            // Step 2: Bootstrap RBAC (sync policies and role metadata)
            await BootstrapRbacAsync();

            logger.LogInformation("Permission system initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize permission system");
            throw;
        }
    }

    /// <summary>
    /// Applies all pending database migrations automatically
    /// </summary>
    private async Task ApplyMigrationsAsync()
    {
        logger.LogInformation("Checking for pending database migrations");

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            var pendingList = pendingMigrations.ToList();

            if (pendingList.Count > 0)
            {
                logger.LogInformation("Found {Count} pending migration(s), applying now", pendingList.Count);

                foreach (var migration in pendingList)
                {
                    logger.LogDebug("  - {Migration}", migration);
                }

                await context.Database.MigrateAsync();
                logger.LogInformation("Successfully applied {Count} migration(s)", pendingList.Count);
            }
            else
            {
                logger.LogInformation("Database is up to date, no pending migrations");
            }
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P07") // 42P07 = relation already exists
        {
            logger.LogWarning("Database tables already exist, skipping migration. This may indicate the migration history is out of sync.");
            logger.LogDebug(ex, "Migration error details");
            // Continue - the tables exist, which is what we need
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply database migrations. Error: {Message}. Inner: {InnerMessage}",
                ex.Message, ex.InnerException?.Message ?? "(none)");
            throw;
        }
    }

    /// <summary>
    /// Bootstraps the RBAC system by syncing policies and role metadata
    /// </summary>
    private async Task BootstrapRbacAsync()
    {
        logger.LogInformation("Bootstrapping RBAC system");

        using var scope = serviceProvider.CreateScope();
        var bootstrap = scope.ServiceProvider.GetRequiredService<PermissionBootstrap>();

        try
        {
            await bootstrap.BootstrapAsync();
            logger.LogInformation("RBAC bootstrap completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to bootstrap RBAC system");
            throw;
        }
    }

    /// <summary>
    /// Verifies that the permission system is properly initialized
    /// </summary>
    public async Task<bool> VerifyInitializationAsync()
    {
        logger.LogInformation("Verifying permission system initialization");

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            // Check if migrations are applied
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogWarning("Found {Count} pending migrations", pendingMigrations.Count());
                return false;
            }

            // Check if Casbin rules table exists and has data
            var casbinRulesCount = await context.CasbinRules.CountAsync();
            logger.LogInformation("Found {Count} Casbin rules in database", casbinRulesCount);

            // Check if role metadata exists
            var roleMetadataCount = await context.RoleMetadata.CountAsync();
            logger.LogInformation("Found {Count} role metadata entries in database", roleMetadataCount);

            var isInitialized = casbinRulesCount > 0 && roleMetadataCount > 0;
            
            if (isInitialized)
            {
                logger.LogInformation("Permission system is properly initialized");
            }
            else
            {
                logger.LogWarning("Permission system may not be fully initialized");
            }

            return isInitialized;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to verify permission system initialization");
            return false;
        }
    }
}

