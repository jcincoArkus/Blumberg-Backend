using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;
using Shared.Enums;

namespace Adapters.Database.Schemas;

/// <summary>
/// Entity configuration for <see cref="SensorType"/> entity
/// </summary>
/// <remarks>
/// Configures the SensorType table with PostgreSQL snake_case naming conventions
/// </remarks>
public class SensorTypeSchema : IEntityTypeConfiguration<SensorType>
{
    /// <summary>
    /// Configures the SensorType entity
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public void Configure(EntityTypeBuilder<SensorType> builder)
    {
        // Table name
        builder.ToTable("sensor_types");

        // Primary key
        builder.HasKey(e => e.Id).HasName("pk_sensor_types_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        // Properties (domain enums stored as string VARCHAR(50); new values do not require migration)
        builder.Property(e => e.Type).HasColumnName("type").IsRequired()
            .HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Unit).HasColumnName("unit").IsRequired()
            .HasConversion<string>().HasMaxLength(50);

        // Timestamps
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(e => e.CreatedAt).IsDescending().HasDatabaseName("ix_sensor_types_created_at_desc");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        // Relationships: SensorType classifies many Sensors
        builder.HasMany(e => e.Sensors)
            .WithOne(e => e.SensorType)
            .HasForeignKey(e => e.SensorTypeId)
            .HasConstraintName("fk_sensors_sensor_type_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
