using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>Entity configuration for <see cref="InventoryProduct"/></summary>
public class InventoryProductSchema : IEntityTypeConfiguration<InventoryProduct>
{
    public void Configure(EntityTypeBuilder<InventoryProduct> builder)
    {
        builder.ToTable("products");

        builder.HasKey(e => e.Id).HasName("pk_products_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(e => e.Sku).HasColumnName("sku").IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.Sku).IsUnique().HasDatabaseName("idx_products_sku");

        builder.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);

        builder.Property(e => e.CategoryId).HasColumnName("category_id").IsRequired();
        builder.HasIndex(e => e.CategoryId).HasDatabaseName("idx_products_category_id");

        builder.Property(e => e.Unit).HasColumnName("unit").IsRequired().HasMaxLength(50);
        builder.Property(e => e.KgPerBox).HasColumnName("kg_per_box").HasPrecision(10, 3);
        builder.Property(e => e.ShelfLifeDays).HasColumnName("shelf_life_days").IsRequired();
        builder.Property(e => e.Price).HasColumnName("price").HasPrecision(12, 2).IsRequired();

        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.Ignore(e => e.DeletedAt);

        builder.HasOne(e => e.Category)
            .WithMany(e => e.Products)
            .HasForeignKey(e => e.CategoryId)
            .HasConstraintName("fk_products_category_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
