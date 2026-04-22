using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;
using Shared.Enums;

namespace Adapters.Database.Schemas;

/// <summary>
/// Entity configuration for <see cref="RecommendedAction"/>.
/// Predefined actions keyed by sensor type and severity; global config (no tenant).
/// </summary>
public class RecommendedActionSchema : IEntityTypeConfiguration<RecommendedAction>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RecommendedAction> builder)
    {
        builder.ToTable("recommended_actions");

        builder.HasKey(e => e.Id).HasName("pk_recommended_actions_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(e => e.SensorTypeId).HasColumnName("sensor_type_id").IsRequired();
        builder.Property(e => e.Severity).HasColumnName("severity").IsRequired()
            .HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Title).HasColumnName("title").IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").IsRequired();
        builder.Property(e => e.DisplayOrder).HasColumnName("display_order").IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();

        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(e => new { e.SensorTypeId, e.Severity }).HasDatabaseName("ix_recommended_actions_sensor_type_id_severity");

        builder.HasOne(e => e.SensorType)
            .WithMany()
            .HasForeignKey(e => e.SensorTypeId)
            .HasConstraintName("fk_recommended_actions_sensor_type_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
