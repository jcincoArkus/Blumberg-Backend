using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>
/// Entity configuration for <see cref="SensorReading"/> entity
/// </summary>
/// <remarks>
/// Configures the SensorReading table with PostgreSQL snake_case naming conventions
/// </remarks>
public class SensorReadingSchema : IEntityTypeConfiguration<SensorReading>
{
    /// <summary>
    /// Configures the SensorReading entity
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public void Configure(EntityTypeBuilder<SensorReading> builder)
    {
        // Table name
        builder.ToTable("sensor_readings");

        // Primary key
        builder.HasKey(e => e.Id).HasName("pk_sensor_readings_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        // Properties
        builder.Property(e => e.Value).HasColumnName("value").IsRequired();
        builder.Property(e => e.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.HasIndex(e => e.OrganizationId).HasDatabaseName("ix_sensor_readings_organization_id");
        builder.Property(e => e.SensorId).HasColumnName("sensor_id").IsRequired();
        builder.HasIndex(e => e.SensorId).HasDatabaseName("ix_sensor_readings_sensor_id");

        // Timestamps
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(e => e.CreatedAt).IsDescending().HasDatabaseName("ix_sensor_readings_created_at_desc");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        // Relationships: SensorReading belongs to Organization
        builder.HasOne(e => e.Organization)
            .WithMany(e => e.SensorReadings)
            .HasForeignKey(e => e.OrganizationId)
            .HasConstraintName("fk_sensor_readings_organization_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Relationships: SensorReading triggers many Alerts
        builder.HasMany(e => e.Alerts)
            .WithOne(e => e.SensorReading)
            .HasForeignKey(e => e.SensorReadingId)
            .HasConstraintName("fk_alerts_sensor_reading_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
