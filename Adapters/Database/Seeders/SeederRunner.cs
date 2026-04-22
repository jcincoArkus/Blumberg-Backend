using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Adapters.Database.Seeders;

/// <summary>
/// Runs all registered seeders
/// </summary>
public static class SeederRunner
{
    /// <summary>
    /// Gets all seeders ordered by their Order property
    /// </summary>
    private static IEnumerable<ISeeder> GetSeeders(ILoggerFactory loggerFactory)
    {
        yield return new SensorTypeSeeder(loggerFactory);
        yield return new RecommendedActionSeeder(loggerFactory);
        yield return new OrganizationSeeder(loggerFactory);
        yield return new SiteSeeder(loggerFactory);
        yield return new EquipmentSeeder(loggerFactory);
        yield return new SensorSeeder(loggerFactory);
        // yield return new SensorReadingSeeder(loggerFactory);
        yield return new AdminSeeder(loggerFactory);
    }

    /// <summary>
    /// Runs all seeders in order
    /// </summary>
    private static async Task RunAsync(ApplicationDbContext context, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("SeederRunner");
        logger.LogInformation("Running seeders");

        foreach (var seeder in GetSeeders(loggerFactory).OrderBy(s => s.Order))
        {
            logger.LogInformation("Running seeder: {SeederName}", seeder.GetType().Name);
            await seeder.SeedAsync(context);
        }

        logger.LogInformation("Seeders completed");
    }

    /// <summary>
    /// Drops the database and recreates the schema by applying all migrations.
    /// Populates __EFMigrationsHistory so migration:down works after nuke/nukeAndPave.
    /// </summary>
    private static async Task NukeAsync(ApplicationDbContext context, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("SeederRunner");

        logger.LogWarning("Dropping database");
        await context.Database.EnsureDeletedAsync();

        logger.LogInformation("Applying all migrations");
        await context.Database.MigrateAsync();

        logger.LogInformation("Database recreated (migration history applied)");
    }

    /// <summary>
    /// Nukes the database and runs all seeders
    /// </summary>
    public static async Task NukeAndPaveAsync(ApplicationDbContext context, ILoggerFactory loggerFactory)
    {
        await NukeAsync(context, loggerFactory);
        await RunAsync(context, loggerFactory);
    }

    /// <summary>
    /// Runs all seeders (no nuke). Idempotent: seeders skip when data already exists.
    /// Use from API startup in Development so dev DB gets seed data without running the CLI.
    /// </summary>
    public static async Task RunSeedersAsync(ApplicationDbContext context, ILoggerFactory loggerFactory)
    {
        await RunAsync(context, loggerFactory);
    }
}

