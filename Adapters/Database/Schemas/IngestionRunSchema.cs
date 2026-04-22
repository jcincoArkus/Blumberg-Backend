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

        builder.Property(e => e.Source).HasColumnName("source").IsRequired()
            .HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Status).HasColumnName("status").IsRequired()
            .HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.TotalRecords).HasColumnName("total_records").IsRequired();
        builder.Property(e => e.AcceptedRecords).HasColumnName("accepted_records").IsRequired();
        builder.Property(e => e.RejectedRecords).HasColumnName("rejected_records").IsRequired();
        builder.Property(e => e.StartedAt).HasColumnName("started_at");
        builder.Property(e => e.CompletedAt).HasColumnName("completed_at");

        builder.Property(e => e.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.HasIndex(e => e.OrganizationId).HasDatabaseName("ix_ingestion_runs_organization_id");

        builder.HasOne(e => e.Organization)
            .WithMany(e => e.IngestionRuns)
            .HasForeignKey(e => e.OrganizationId)
            .HasConstraintName("fk_ingestion_runs_organization_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to SensorReadings is configured in SensorReadingSchema (FK on SensorReading)
    }
}
