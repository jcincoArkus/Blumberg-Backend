using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>
/// Entity configuration for <see cref="IngestionRejectedReading"/> entity
/// </summary>
public class IngestionRejectedReadingSchema : IEntityTypeConfiguration<IngestionRejectedReading>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<IngestionRejectedReading> builder)
    {
        builder.ToTable("ingestion_run_rejected_readings");

        builder.HasKey(e => e.Id).HasName("pk_ingestion_run_rejected_readings_id");
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.IngestionRunId).HasColumnName("ingestion_run_id").IsRequired();
        builder.Property(e => e.RowIndex).HasColumnName("row_index").IsRequired();
        builder.Property(e => e.SensorId).HasColumnName("sensor_id");
        builder.Property(e => e.RejectionReason).HasColumnName("rejection_reason").IsRequired().HasMaxLength(500);

        builder.HasIndex(e => e.IngestionRunId).HasDatabaseName("ix_ingestion_run_rejected_readings_ingestion_run_id");
        builder.HasIndex(e => e.SensorId).HasDatabaseName("ix_ingestion_run_rejected_readings_sensor_id");

        builder.HasOne(e => e.IngestionRun)
            .WithMany(e => e.RejectedReadings)
            .HasForeignKey(e => e.IngestionRunId)
            .HasConstraintName("fk_ingestion_run_rejected_readings_ingestion_run_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
