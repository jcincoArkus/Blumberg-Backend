using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Adapters.Config;

namespace Adapters.Database;

/// <summary>
/// Factory for creating ApplicationDbContext at design-time and runtime
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private static DbContextOptions<ApplicationDbContext>? _cachedOptions;

    /// <summary>
    /// Creates a new ApplicationDbContext instance using configuration from .env.
    /// Uses a design-time tenant context (no request scope); for migrations and tooling only.
    /// </summary>
    public static ApplicationDbContext Create()
    {
        var options = GetOptions();
        return new ApplicationDbContext(options, new DesignTimeTenantContext());
    }

    /// <summary>
    /// Gets the DbContextOptions, cached for reuse
    /// </summary>
    public static DbContextOptions<ApplicationDbContext> GetOptions()
    {
        if (_cachedOptions != null)
            return _cachedOptions;

        var config = ConfigLoader.LoadForDatabaseOperations();
        var connectionString = config.Database.GetConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        _cachedOptions = optionsBuilder.Options;
        return _cachedOptions;
    }

    /// <summary>
    /// Gets the connection string from configuration
    /// </summary>
    public static string GetConnectionString()
    {
        var config = ConfigLoader.LoadForDatabaseOperations();
        return config.Database.GetConnectionString();
    }

    /// <summary>
    /// Design-time factory method (used by EF Core tools)
    /// </summary>
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        return Create();
    }
}

