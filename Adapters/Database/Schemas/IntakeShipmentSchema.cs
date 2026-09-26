using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>Entity configuration for <see cref="IntakeShipment"/></summary>
public class IntakeShipmentSchema : IEntityTypeConfiguration<IntakeShipment>
{
    public void Configure(EntityTypeBuilder<IntakeShipment> builder)
    {
        builder.ToTable("intake_shipments");

        builder.HasKey(e => e.Id).HasName("pk_intake_shipments_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(e => e.PoReference).HasColumnName("po_reference").IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.PoReference).IsUnique();

        builder.Property(e => e.SupplierId).HasColumnName("supplier_id").IsRequired();
        builder.HasIndex(e => e.SupplierId).HasDatabaseName("idx_intake_shipments_supplier_id");

        builder.Property(e => e.Vehicle).HasColumnName("vehicle").HasMaxLength(200);
        builder.Property(e => e.Driver).HasColumnName("driver").HasMaxLength(200);

        builder.Property(e => e.SiteId).HasColumnName("site_id").IsRequired();
        builder.HasIndex(e => e.SiteId).HasDatabaseName("idx_intake_shipments_site_id");

        builder.Property(e => e.ReceivingZone).HasColumnName("receiving_zone").IsRequired().HasMaxLength(100);
        builder.Property(e => e.ColdChainTempC).HasColumnName("cold_chain_temp_c").HasPrecision(5, 2);

        builder.Property(e => e.ArrivedAt).HasColumnName("arrived_at").IsRequired();
        builder.HasIndex(e => e.ArrivedAt).IsDescending().HasDatabaseName("idx_intake_shipments_arrived_at");

        builder.Property(e => e.ReceivedBy).HasColumnName("received_by").IsRequired().HasMaxLength(200);
        builder.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(50);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.Ignore(e => e.DeletedAt);

        builder.HasOne(e => e.Supplier)
            .WithMany(e => e.IntakeShipments)
            .HasForeignKey(e => e.SupplierId)
            .HasConstraintName("fk_intake_shipments_supplier_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Site)
            .WithMany(e => e.IntakeShipments)
            .HasForeignKey(e => e.SiteId)
            .HasConstraintName("fk_intake_shipments_site_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Lines)
            .WithOne(e => e.Shipment)
            .HasForeignKey(e => e.ShipmentId)
            .HasConstraintName("fk_intake_shipment_lines_shipment_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
