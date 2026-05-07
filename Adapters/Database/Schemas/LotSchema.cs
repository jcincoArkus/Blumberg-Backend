using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>Entity configuration for <see cref="Lot"/></summary>
public class LotSchema : IEntityTypeConfiguration<Lot>
{
    public void Configure(EntityTypeBuilder<Lot> builder)
    {
        builder.ToTable("lots");

        builder.HasKey(e => e.LotCode).HasName("pk_lots_lot_code");
        builder.Property(e => e.LotCode).HasColumnName("lot_code").HasMaxLength(50).IsRequired();

        builder.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
        builder.HasIndex(e => e.ProductId).HasDatabaseName("idx_lots_product_id");

        builder.Property(e => e.Qty).HasColumnName("qty").HasPrecision(12, 3).IsRequired();
        builder.Property(e => e.Unit).HasColumnName("unit").IsRequired().HasMaxLength(50);
        builder.Property(e => e.EntryAt).HasColumnName("entry_at").IsRequired();
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.HasIndex(e => e.ExpiresAt).HasDatabaseName("idx_lots_expires_at");
        builder.HasIndex(e => e.EntryAt).HasDatabaseName("idx_lots_entry_at");

        builder.Property(e => e.SiteId).HasColumnName("site_id").IsRequired();
        builder.HasIndex(e => e.SiteId).HasDatabaseName("idx_lots_site_id");

        builder.Property(e => e.Zone).HasColumnName("zone").IsRequired().HasMaxLength(100);
        builder.Property(e => e.SupplierId).HasColumnName("supplier_id");
        builder.Property(e => e.CostPerUnit).HasColumnName("cost_per_unit").HasPrecision(12, 4).IsRequired();

        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(e => e.Product)
            .WithMany(e => e.Lots)
            .HasForeignKey(e => e.ProductId)
            .HasConstraintName("fk_lots_product_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Site)
            .WithMany(e => e.Lots)
            .HasForeignKey(e => e.SiteId)
            .HasConstraintName("fk_lots_site_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Supplier)
            .WithMany(e => e.Lots)
            .HasForeignKey(e => e.SupplierId)
            .HasConstraintName("fk_lots_supplier_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
