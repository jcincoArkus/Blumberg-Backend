using Microsoft.EntityFrameworkCore;

namespace Adapters.Database.Seeders;

/// <summary>
/// Runs all registered seeders
/// </summary>
public static class SeederRunner
{
    /// <summary>
    /// Gets all seeders ordered by their Order property
    /// </summary>
    private static IEnumerable<ISeeder> GetSeeders()
    {
        // Add new seeders here in the future
        yield return new AdminSeeder();
    }

    /// <summary>
    /// Runs all seeders in order
    /// </summary>
    public static async Task RunAsync(ApplicationDbContext context)
    {
        Console.WriteLine("Running seeders...");

        foreach (var seeder in GetSeeders().OrderBy(s => s.Order))
        {
            Console.WriteLine($"  [{seeder.GetType().Name}]");
            await seeder.SeedAsync(context);
        }

        Console.WriteLine("Seeders completed.");
    }

    /// <summary>
    /// Drops all tables and recreates the database schema
    /// </summary>
    public static async Task NukeAsync(ApplicationDbContext context)
    {
        Console.WriteLine("Dropping database...");
        await context.Database.EnsureDeletedAsync();

        Console.WriteLine("Creating database...");
        await context.Database.EnsureCreatedAsync();

        Console.WriteLine("Database recreated.");
    }

    /// <summary>
    /// Nukes the database and runs all seeders
    /// </summary>
    public static async Task NukeAndPaveAsync(ApplicationDbContext context)
    {
        await NukeAsync(context);
        await RunAsync(context);
    }
}

