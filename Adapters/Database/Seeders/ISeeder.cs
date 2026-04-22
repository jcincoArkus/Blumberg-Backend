namespace Adapters.Database.Seeders;

/// <summary>
/// Interface for database seeders
/// </summary>
public interface ISeeder
{
    /// <summary>
    /// Order in which this seeder should run (lower = first)
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Seeds data into the database
    /// </summary>
    Task SeedAsync(ApplicationDbContext context);
}

