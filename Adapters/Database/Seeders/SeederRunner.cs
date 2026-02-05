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
        yield return new OrganizationSeeder(loggerFactory);
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
    /// Drops all tables and recreates the database schema
    /// </summary>
    private static async Task NukeAsync(ApplicationDbContext context, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("SeederRunner");

        logger.LogWarning("Dropping database");
        await context.Database.EnsureDeletedAsync();

        logger.LogInformation("Creating database");
        await context.Database.EnsureCreatedAsync();

        logger.LogInformation("Database recreated");
    }

    /// <summary>
    /// Nukes the database and runs all seeders
    /// </summary>
    public static async Task NukeAndPaveAsync(ApplicationDbContext context, ILoggerFactory loggerFactory)
    {
        await NukeAsync(context, loggerFactory);
        await RunAsync(context, loggerFactory);
    }
}

