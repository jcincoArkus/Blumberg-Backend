using Microsoft.EntityFrameworkCore;
using Shared.Entity;

namespace Adapters.Database;

/// <summary>
/// Main application database context for PostgreSQL
/// </summary>
/// <remarks>
/// This DbContext manages all entity sets and database operations.
/// Configured to use PostgreSQL with Npgsql provider.
/// </remarks>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the ApplicationDbContext
    /// </summary>
    /// <param name="options">DbContext options</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Admins DbSet
    /// </summary>
    public DbSet<Admin> Admins => Set<Admin>();

    /// <summary>
    /// Configures the model and entity relationships
    /// </summary>
    /// <param name="modelBuilder">Model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        // Entity configurations will be added in separate files (e.g., AdminConfiguration.cs)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    /// <summary>
    /// Saves all changes made in this context to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of state entries written to the database</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Add automatic timestamp handling here if needed
        // Example: Set CreatedAt, UpdatedAt for entities with BaseEntity

        return await base.SaveChangesAsync(cancellationToken);
    }
}

