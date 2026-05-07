using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>Entity configuration for <see cref="IntakeShipmentLine"/></summary>
public class IntakeShipmentLineSchema : IEntityTypeConfiguration<IntakeShipmentLine>
{
    public void Configure(EntityTypeBuilder<IntakeShipmentLine> builder)
    {
        builder.ToTable("intake_shipment_lines");

        builder.HasKey(e => e.Id).HasName("pk_intake_shipment_lines_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(e => e.ShipmentId).HasColumnName("shipment_id").IsRequired();
        builder.HasIndex(e => e.ShipmentId).HasDatabaseName("idx_intake_lines_shipment_id");

        builder.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
        builder.HasIndex(e => e.ProductId).HasDatabaseName("idx_intake_lines_product_id");

        builder.Property(e => e.LotCode).HasColumnName("lot_code").HasMaxLength(50);
        builder.Property(e => e.Qty).HasColumnName("qty").HasPrecision(12, 3).IsRequired();
        builder.Property(e => e.Unit).HasColumnName("unit").IsRequired().HasMaxLength(50);
        builder.Property(e => e.CostPerUnit).HasColumnName("cost_per_unit").HasPrecision(12, 4).IsRequired();

        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

        // No updated_at or deleted_at on this table
        builder.Ignore(e => e.UpdatedAt);
        builder.Ignore(e => e.DeletedAt);

        builder.HasOne(e => e.Shipment)
            .WithMany(e => e.Lines)
            .HasForeignKey(e => e.ShipmentId)
            .HasConstraintName("fk_intake_shipment_lines_shipment_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Product)
            .WithMany(e => e.ShipmentLines)
            .HasForeignKey(e => e.ProductId)
            .HasConstraintName("fk_intake_shipment_lines_product_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Lot)
            .WithMany(e => e.ShipmentLines)
            .HasForeignKey(e => e.LotCode)
            .HasConstraintName("fk_intake_shipment_lines_lot_code")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
