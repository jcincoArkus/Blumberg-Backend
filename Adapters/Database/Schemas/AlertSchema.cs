using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>
/// Entity configuration for <see cref="Alert"/> entity
/// </summary>
/// <remarks>
/// Configures the Alerts table with PostgreSQL snake_case naming conventions
/// </remarks>
public class AlertSchema : IEntityTypeConfiguration<Alert>
{
    /// <summary>
    /// Configures the Alert entity
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        // Table name
        builder.ToTable("alerts");

        // Primary key
        builder.HasKey(e => e.Id).HasName("pk_alerts_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        // Foreign keys
        builder.Property(e => e.SensorId).HasColumnName("sensor_id").IsRequired();
        builder.Property(e => e.EquipmentId).HasColumnName("equipment_id").IsRequired();
        builder.Property(e => e.SiteId).HasColumnName("site_id").IsRequired();
        builder.Property(e => e.OrganizationId).HasColumnName("organization_id").IsRequired();

        // Alert data
        builder.Property(e => e.Severity).HasColumnName("severity").IsRequired();
        builder.Property(e => e.TriggeredValue).HasColumnName("triggered_value").IsRequired();
        builder.Property(e => e.ThresholdMin).HasColumnName("threshold_min").IsRequired();
        builder.Property(e => e.ThresholdMax).HasColumnName("threshold_max").IsRequired();
        builder.Property(e => e.TriggeredAt).HasColumnName("triggered_at").IsRequired();
        builder.Property(e => e.Status).HasColumnName("status").IsRequired();
        builder.Property(e => e.ResolvedAt).HasColumnName("resolved_at");

        // Timestamps
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(e => e.CreatedAt).IsDescending().HasDatabaseName("ix_alerts_created_at_desc");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        // Indexes
        builder.HasIndex(e => e.SensorId).HasDatabaseName("ix_alerts_sensor_id");
        builder.HasIndex(e => e.OrganizationId).HasDatabaseName("ix_alerts_organization_id");
        builder.HasIndex(e => new { e.SensorId, e.Status }).HasDatabaseName("ix_alerts_sensor_id_status");

        // Relationships
        builder.HasOne(e => e.Sensor)
            .WithMany()
            .HasForeignKey(e => e.SensorId)
            .HasConstraintName("fk_alerts_sensor_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Equipment)
            .WithMany()
            .HasForeignKey(e => e.EquipmentId)
            .HasConstraintName("fk_alerts_equipment_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Site)
            .WithMany()
            .HasForeignKey(e => e.SiteId)
            .HasConstraintName("fk_alerts_site_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Organization)
            .WithMany()
            .HasForeignKey(e => e.OrganizationId)
            .HasConstraintName("fk_alerts_organization_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
