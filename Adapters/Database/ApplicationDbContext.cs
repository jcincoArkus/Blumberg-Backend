using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using Shared.Entity;
using Shared.Enums;

namespace Adapters.Database;

/// <summary>
/// Main application database context for PostgreSQL.
/// Applies global query filters: soft delete (DeletedAt == null) and tenant scope (OrganizationId) where applicable.
/// </summary>
public class ApplicationDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// Initializes a new instance of the ApplicationDbContext
    /// </summary>
    /// <param name="options">DbContext options</param>
    /// <param name="tenantContext">Current tenant context for request-scoped filtering</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Gets or sets the Admins DbSet
    /// </summary>
    public DbSet<Admin> Admins => Set<Admin>();

    /// <summary>
    /// Gets or sets the Organizations DbSet
    /// </summary>
    public DbSet<Organization> Organizations => Set<Organization>();

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
    /// Gets or sets the AlertEvents DbSet (append-only lifecycle events per alert)
    /// </summary>
    public DbSet<AlertEvent> AlertEvents => Set<AlertEvent>();

    /// <summary>
    /// Gets or sets the IngestionRuns DbSet
    /// </summary>
    public DbSet<IngestionRun> IngestionRuns => Set<IngestionRun>();

    /// <summary>
    /// Gets or sets the IngestionRejectedReadings DbSet
    /// </summary>
    public DbSet<IngestionRejectedReading> IngestionRejectedReadings => Set<IngestionRejectedReading>();

    /// <summary>
    /// Gets or sets the API keys DbSet (no global filter; used for auth lookup by hash)
    /// </summary>
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    /// <summary>
    /// Gets or sets the CasbinRules DbSet for authorization policies
    /// </summary>
    public DbSet<CasbinRule> CasbinRules => Set<CasbinRule>();

    /// <summary>
    /// Gets or sets the RoleMetadata DbSet for role display information
    /// </summary>
    public DbSet<RoleMetadata> RoleMetadata => Set<RoleMetadata>();

    /// <summary>
    /// Gets or sets the RecommendedActions DbSet (predefined actions by sensor type + severity).
    /// </summary>
    public DbSet<RecommendedAction> RecommendedActions => Set<RecommendedAction>();

    public DbSet<InventoryCategory> InventoryCategories => Set<InventoryCategory>();
    public DbSet<InventorySite> InventorySites => Set<InventorySite>();
    public DbSet<SiteZone> SiteZones => Set<SiteZone>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<InventoryProduct> InventoryProducts => Set<InventoryProduct>();
    public DbSet<Lot> Lots => Set<Lot>();
    public DbSet<Movement> Movements => Set<Movement>();
    public DbSet<IntakeShipment> IntakeShipments => Set<IntakeShipment>();
    public DbSet<IntakeShipmentLine> IntakeShipmentLines => Set<IntakeShipmentLine>();

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
            typeof(SensorHealthStatus),
            typeof(AlertSeverity),
            typeof(AlertStatus),
            typeof(AlertEventType)
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

        // Global query filters: soft delete + tenant scope (evaluated at query time; no cross-tenant data access)
        modelBuilder.Entity<Admin>().HasQueryFilter(e => e.DeletedAt == null && _tenantContext.CurrentOrganizationId != null && e.OrganizationId == _tenantContext.CurrentOrganizationId);
        modelBuilder.Entity<Site>().HasQueryFilter(e => e.DeletedAt == null && _tenantContext.CurrentOrganizationId != null && e.OrganizationId == _tenantContext.CurrentOrganizationId);
        modelBuilder.Entity<Equipment>().HasQueryFilter(e => e.DeletedAt == null && _tenantContext.CurrentOrganizationId != null && e.OrganizationId == _tenantContext.CurrentOrganizationId);
        modelBuilder.Entity<Sensor>().HasQueryFilter(e => e.DeletedAt == null && _tenantContext.CurrentOrganizationId != null && e.OrganizationId == _tenantContext.CurrentOrganizationId);
        modelBuilder.Entity<SensorReading>().HasQueryFilter(e => e.DeletedAt == null && _tenantContext.CurrentOrganizationId != null && e.OrganizationId == _tenantContext.CurrentOrganizationId);

        // Soft delete only (no OrganizationId)
        modelBuilder.Entity<Organization>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<SensorType>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Threshold>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Alert>().HasQueryFilter(e => e.DeletedAt == null && _tenantContext.CurrentOrganizationId != null && e.OrganizationId == _tenantContext.CurrentOrganizationId);
        modelBuilder.Entity<AlertEvent>().HasQueryFilter(e => e.DeletedAt == null && _tenantContext.CurrentOrganizationId != null && e.OrganizationId == _tenantContext.CurrentOrganizationId);
        // IngestionRun: tenant-scoped by OrganizationId
        modelBuilder.Entity<IngestionRun>().HasQueryFilter(e => e.DeletedAt == null && _tenantContext.CurrentOrganizationId != null && e.OrganizationId == _tenantContext.CurrentOrganizationId);
        // RecommendedAction: global config (no tenant), soft delete only
        modelBuilder.Entity<RecommendedAction>().HasQueryFilter(e => e.DeletedAt == null);
    }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// Validates tenant scope: no cross-tenant writes; sets OrganizationId on new tenant-scoped entities.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of state entries written to the database</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var orgId = _tenantContext.CurrentOrganizationId;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                if (entry.Entity is Admin a)
                    ValidateAndSetTenant(a.OrganizationId, () => a.OrganizationId = orgId!.Value, orgId, nameof(Admin));
                else if (entry.Entity is Site s)
                    ValidateAndSetTenant(s.OrganizationId, () => s.OrganizationId = orgId!.Value, orgId, nameof(Site));
                else if (entry.Entity is Equipment eq)
                    ValidateAndSetTenant(eq.OrganizationId, () => eq.OrganizationId = orgId!.Value, orgId, nameof(Equipment));
                else if (entry.Entity is Sensor sn)
                    ValidateAndSetTenant(sn.OrganizationId, () => sn.OrganizationId = orgId!.Value, orgId, nameof(Sensor));
                else if (entry.Entity is SensorReading sr)
                    ValidateAndSetTenant(sr.OrganizationId, () => sr.OrganizationId = orgId!.Value, orgId, nameof(SensorReading));
                else if (entry.Entity is IngestionRun run)
                    ValidateAndSetTenant(run.OrganizationId, () => run.OrganizationId = orgId!.Value, orgId, nameof(IngestionRun));
                else if (entry.Entity is Alert alert)
                    ValidateAndSetTenant(alert.OrganizationId, () => alert.OrganizationId = orgId!.Value, orgId, nameof(Alert));
                else if (entry.Entity is AlertEvent alertEvent)
                    ValidateAndSetTenant(alertEvent.OrganizationId, () => alertEvent.OrganizationId = orgId!.Value, orgId, nameof(AlertEvent));
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateAndSetTenant(Guid entityOrgId, Action setOrgId, Guid? currentOrgId, string entityName)
    {
        // No tenant context (e.g. design-time, CLI seeders): allow only if entity already has OrganizationId set
        if (currentOrgId == null)
        {
            if (entityOrgId == Guid.Empty)
                throw new InvalidOperationException($"Tenant context is required to create or update {entityName}, or set OrganizationId explicitly (e.g. in seeders).");
            return;
        }
        if (entityOrgId == Guid.Empty)
            setOrgId();
        else if (entityOrgId != currentOrgId)
            throw new InvalidOperationException($"Cross-tenant access is not allowed for {entityName}.");
    }
}

