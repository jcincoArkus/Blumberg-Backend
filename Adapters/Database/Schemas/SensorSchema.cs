using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;
using Shared.Enums;

namespace Adapters.Database.Schemas;

/// <summary>
/// Entity configuration for <see cref="Sensor"/> entity
/// </summary>
/// <remarks>
/// Configures the Sensor table with PostgreSQL snake_case naming conventions
/// </remarks>
public class SensorSchema : IEntityTypeConfiguration<Sensor>
{
    /// <summary>
    /// Configures the Sensor entity
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        // Table name
        builder.ToTable("sensors");

        // Primary key
        builder.HasKey(e => e.Id).HasName("pk_sensors_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        // Properties
        builder.Property(e => e.Serial).HasColumnName("serial").IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.Serial).HasDatabaseName("ix_sensors_serial");
        builder.Property(e => e.Status).HasColumnName("status").IsRequired()
            .HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.HasIndex(e => e.OrganizationId).HasDatabaseName("ix_sensors_organization_id");
        builder.Property(e => e.EquipmentId).HasColumnName("equipment_id").IsRequired();
        builder.HasIndex(e => e.EquipmentId).HasDatabaseName("ix_sensors_equipment_id");
        builder.Property(e => e.SensorTypeId).HasColumnName("sensor_type_id").IsRequired();
        builder.HasIndex(e => e.SensorTypeId).HasDatabaseName("ix_sensors_sensor_type_id");
        builder.Property(e => e.ThresholdId).HasColumnName("threshold_id").IsRequired();
        builder.HasIndex(e => e.ThresholdId).IsUnique().HasDatabaseName("ix_sensors_threshold_id_unique");
        builder.Property(e => e.LastSeenAt).HasColumnName("last_seen_at");
        builder.Property(e => e.FirstOutOfRangeAt).HasColumnName("first_out_of_range_at");

        // Timestamps
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(e => e.CreatedAt).IsDescending().HasDatabaseName("ix_sensors_created_at_desc");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        // Relationships: Sensor belongs to Organization
        builder.HasOne(e => e.Organization)
            .WithMany(e => e.Sensors)
            .HasForeignKey(e => e.OrganizationId)
            .HasConstraintName("fk_sensors_organization_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Relationships: Sensor is configured by one Threshold (1:1)
        builder.HasOne(e => e.Threshold)
            .WithOne(e => e.Sensor)
            .HasForeignKey<Sensor>(e => e.ThresholdId)
            .HasConstraintName("fk_sensors_threshold_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Relationships: Sensor produces many SensorReadings
        builder.HasMany(e => e.SensorReadings)
            .WithOne(e => e.Sensor)
            .HasForeignKey(e => e.SensorId)
            .HasConstraintName("fk_sensor_readings_sensor_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
