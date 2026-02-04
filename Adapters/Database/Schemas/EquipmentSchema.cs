using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>
/// Entity configuration for <see cref="Equipment"/> entity
/// </summary>
/// <remarks>
/// Configures the Equipment table with PostgreSQL snake_case naming conventions
/// </remarks>
public class EquipmentSchema : IEntityTypeConfiguration<Equipment>
{
    /// <summary>
    /// Configures the Equipment entity
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        // Table name
        builder.ToTable("equipment");

        // Primary key
        builder.HasKey(e => e.Id).HasName("pk_equipment_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        // Properties
        builder.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(e => e.EquipmentType).HasColumnName("equipment_type").IsRequired().HasMaxLength(100);
        builder.Property(e => e.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.HasIndex(e => e.OrganizationId).HasDatabaseName("ix_equipment_organization_id");
        builder.Property(e => e.SiteId).HasColumnName("site_id").IsRequired();
        builder.HasIndex(e => e.SiteId).HasDatabaseName("ix_equipment_site_id");

        // Timestamps
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(e => e.CreatedAt).IsDescending().HasDatabaseName("ix_equipment_created_at_desc");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        // Relationships: Equipment belongs to Organization
        builder.HasOne(e => e.Organization)
            .WithMany(e => e.Equipment)
            .HasForeignKey(e => e.OrganizationId)
            .HasConstraintName("fk_equipment_organization_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Relationships: Equipment contains many Sensors
        builder.HasMany(e => e.Sensors)
            .WithOne(e => e.Equipment)
            .HasForeignKey(e => e.EquipmentId)
            .HasConstraintName("fk_sensors_equipment_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}