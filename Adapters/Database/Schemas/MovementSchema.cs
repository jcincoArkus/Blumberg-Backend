using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>Entity configuration for <see cref="Movement"/></summary>
public class MovementSchema : IEntityTypeConfiguration<Movement>
{
    public void Configure(EntityTypeBuilder<Movement> builder)
    {
        builder.ToTable("movements");

        builder.HasKey(e => e.Id).HasName("pk_movements_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(e => e.Type).HasColumnName("type").IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.Type).HasDatabaseName("idx_movements_type");

        builder.Property(e => e.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.HasIndex(e => e.OccurredAt).IsDescending().HasDatabaseName("idx_movements_occurred_at");

        builder.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
        builder.HasIndex(e => e.ProductId).HasDatabaseName("idx_movements_product_id");

        builder.Property(e => e.Qty).HasColumnName("qty").HasPrecision(12, 3).IsRequired();
        builder.Property(e => e.Unit).HasColumnName("unit").IsRequired().HasMaxLength(50);

        builder.Property(e => e.LotCode).HasColumnName("lot_code").HasMaxLength(50);
        builder.HasIndex(e => e.LotCode).HasDatabaseName("idx_movements_lot_code");

        builder.Property(e => e.SiteId).HasColumnName("site_id").IsRequired();
        builder.HasIndex(e => e.SiteId).HasDatabaseName("idx_movements_site_id");

        builder.Property(e => e.DestSiteId).HasColumnName("dest_site_id");
        builder.Property(e => e.PerformedBy).HasColumnName("performed_by").IsRequired().HasMaxLength(200);
        builder.Property(e => e.Note).HasColumnName("note");

        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

        // movements is append-only — no updated_at or deleted_at columns
        builder.Ignore(e => e.UpdatedAt);
        builder.Ignore(e => e.DeletedAt);

        builder.HasOne(e => e.Product)
            .WithMany(e => e.Movements)
            .HasForeignKey(e => e.ProductId)
            .HasConstraintName("fk_movements_product_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Lot)
            .WithMany(e => e.Movements)
            .HasForeignKey(e => e.LotCode)
            .HasConstraintName("fk_movements_lot_code")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Site)
            .WithMany(e => e.Movements)
            .HasForeignKey(e => e.SiteId)
            .HasConstraintName("fk_movements_site_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DestSite)
            .WithMany()
            .HasForeignKey(e => e.DestSiteId)
            .HasConstraintName("fk_movements_dest_site_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
