using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>
/// Entity configuration for <see cref="Organization"/> entity
/// </summary>
/// <remarks>
/// Configures the organizations table with PostgreSQL snake_case naming conventions
/// </remarks>
public class OrganizationSchema : IEntityTypeConfiguration<Organization>
{
    /// <summary>
    /// Configures the Organization entity
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        // Table name
        builder.ToTable("organizations");

        // Primary key
        builder.HasKey(e => e.Id).HasName("pk_organizations_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        // Properties
        builder.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(e => e.Slug).HasColumnName("slug").IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.Slug).IsUnique().HasDatabaseName("ix_organizations_slug_unique");

        // Timestamps
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(e => e.CreatedAt).IsDescending().HasDatabaseName("ix_organizations_created_at_desc");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        // Relationships: Organization has many Sites
        builder.HasMany(e => e.Sites)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrganizationId)
            .HasConstraintName("fk_sites_organization_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Relationships: Organization has many Equipment
        builder.HasMany(e => e.Equipment)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrganizationId)
            .HasConstraintName("fk_equipment_organization_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Relationships: Organization has many Sensors
        builder.HasMany(e => e.Sensors)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrganizationId)
            .HasConstraintName("fk_sensors_organization_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Relationships: Organization has many SensorReadings
        builder.HasMany(e => e.SensorReadings)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrganizationId)
            .HasConstraintName("fk_sensor_readings_organization_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Relationships: Organization has many Admins
        builder.HasMany(e => e.Admins)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrganizationId)
            .HasConstraintName("fk_admins_organization_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Relationships: Organization has many IngestionRuns
        builder.HasMany(e => e.IngestionRuns)
            .WithOne(e => e.Organization)
            .HasForeignKey(e => e.OrganizationId)
            .HasConstraintName("fk_ingestion_runs_organization_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
