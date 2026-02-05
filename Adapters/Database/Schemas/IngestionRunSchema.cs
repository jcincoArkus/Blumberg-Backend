
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>
/// Entity configuration for <see cref="IngestionRun"/> entity
/// </summary>
public class IngestionRunSchema : IEntityTypeConfiguration<IngestionRun>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<IngestionRun> builder)
    {
        builder.ToTable("ingestion_runs");

        builder.HasKey(e => e.Id).HasName("pk_ingestion_runs_id");
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        // Relationship to SensorReadings is configured in SensorReadingSchema (FK on SensorReading)
    }
}
