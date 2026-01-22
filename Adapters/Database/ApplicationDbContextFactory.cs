using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Adapters.Config;

namespace Adapters.Database;

/// <summary>
/// Factory for creating ApplicationDbContext at design-time (for EF Core tools)
/// This is only used to generate migrations, not at runtime
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Use ConfigLoader to load all configurations (including .env file)
        var config = ConfigLoader.Load();
        var connectionString = config.Database.GetConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

