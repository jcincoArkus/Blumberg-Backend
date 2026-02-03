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

        // Properties
        builder.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasColumnName("description").IsRequired().HasMaxLength(2000);
        builder.Property(e => e.SensorReadingId).HasColumnName("sensor_reading_id").IsRequired();
        builder.HasIndex(e => e.SensorReadingId).HasDatabaseName("ix_alerts_sensor_reading_id");

        // Enums
        // Note: EF Core will store enums as integers by default unless configured otherwise.
        builder.Property<Alert.AlertType>("type").HasColumnName("type").IsRequired();
        builder.Property<Alert.AlertStatus>("status").HasColumnName("status").IsRequired();

        // Timestamps
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(e => e.CreatedAt).IsDescending().HasDatabaseName("ix_alerts_created_at_desc");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");
    }
}

