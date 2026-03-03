using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>
/// Entity configuration for <see cref="AlertEvent"/> entity
/// </summary>
/// <remarks>
/// Configures the alert_events table with PostgreSQL snake_case naming conventions.
/// Append-only events for alert lifecycle (triggered, acknowledged, resolved, note, system_update).
/// </remarks>
public class AlertEventSchema : IEntityTypeConfiguration<AlertEvent>
{
    /// <summary>
    /// Configures the AlertEvent entity
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public void Configure(EntityTypeBuilder<AlertEvent> builder)
    {
        // Table name
        builder.ToTable("alert_events");

        // Primary key
        builder.HasKey(e => e.Id).HasName("pk_alert_events_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        // Foreign keys
        builder.Property(e => e.AlertId).HasColumnName("alert_id").IsRequired();
        builder.Property(e => e.OrganizationId).HasColumnName("organization_id").IsRequired();

        // Event data
        builder.Property(e => e.EventType).HasColumnName("event_type").IsRequired();
        builder.Property(e => e.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").IsRequired();
        builder.Property(e => e.ActorId).HasColumnName("actor_id");

        // Timestamps
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        // Indexes
        builder.HasIndex(e => e.AlertId).HasDatabaseName("ix_alert_events_alert_id");
        builder.HasIndex(e => e.OrganizationId).HasDatabaseName("ix_alert_events_organization_id");
        builder.HasIndex(e => e.OccurredAt).HasDatabaseName("ix_alert_events_occurred_at");

        // Relationships
        builder.HasOne(e => e.Alert)
            .WithMany(a => a.Events)
            .HasForeignKey(e => e.AlertId)
            .HasConstraintName("fk_alert_events_alert_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Organization)
            .WithMany()
            .HasForeignKey(e => e.OrganizationId)
            .HasConstraintName("fk_alert_events_organization_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
