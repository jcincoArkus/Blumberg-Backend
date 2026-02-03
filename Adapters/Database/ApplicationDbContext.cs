using Microsoft.EntityFrameworkCore;
using Shared.Entity;
using Shared.Enums;

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
    /// Gets or sets the Sites DbSet
    /// </summary>
    public DbSet<Site> Sites => Set<Site>();

    /// <summary>
    /// Gets or sets the Equipment DbSet
    /// </summary>
    public DbSet<Equipment> Equipment => Set<Equipment>();

    /// <summary>
    /// Gets or sets the Sensors DbSet
    /// </summary>
    public DbSet<Sensor> Sensors => Set<Sensor>();

    /// <summary>
    /// Gets or sets the SensorTypes DbSet
    /// </summary>
    public DbSet<SensorType> SensorTypes => Set<SensorType>();

    /// <summary>
    /// Gets or sets the Thresholds DbSet
    /// </summary>
    public DbSet<Threshold> Thresholds => Set<Threshold>();

    /// <summary>
    /// Gets or sets the SensorReadings DbSet
    /// </summary>
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();

    /// <summary>
    /// Gets or sets the Alerts DbSet
    /// </summary>
    public DbSet<Alert> Alerts => Set<Alert>();

    /// <summary>
    /// Configures the model and entity relationships
    /// </summary>
    /// <param name="modelBuilder">Model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Domain enums: store as string VARCHAR(50) so adding new enum values does NOT require a migration
        var domainEnumTypes = new[]
        {
            typeof(SensorStatus),
            typeof(SensorTypeKind),
            typeof(Unit),
            typeof(IngestionSource),
            typeof(IngestionStatus),
            typeof(SensorHealthStatus)
        };
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType.IsEnum && domainEnumTypes.Contains(property.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasConversion<string>()
                        .HasMaxLength(50);
                }
            }
        }
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

