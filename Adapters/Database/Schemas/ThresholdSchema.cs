using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>
/// Entity configuration for <see cref="Threshold"/> entity
/// </summary>
/// <remarks>
/// Configures the Threshold table with PostgreSQL snake_case naming conventions
/// </remarks>
public class ThresholdSchema : IEntityTypeConfiguration<Threshold>
{
    /// <summary>
    /// Configures the Threshold entity
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public void Configure(EntityTypeBuilder<Threshold> builder)
    {
        // Table name
        builder.ToTable("thresholds");

        // Primary key
        builder.HasKey(e => e.Id).HasName("pk_thresholds_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        // Properties
        builder.Property(e => e.Min).HasColumnName("min").IsRequired();
        builder.Property(e => e.Max).HasColumnName("max").IsRequired();

        // Duration (stored as bigint microseconds by default for Npgsql TimeSpan mapping)
        builder.Property(e => e.Duration).HasColumnName("duration").IsRequired();

        // Timestamps
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(e => e.CreatedAt).IsDescending().HasDatabaseName("ix_thresholds_created_at_desc");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");
    }
}

