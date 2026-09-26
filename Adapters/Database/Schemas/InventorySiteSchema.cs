using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>Entity configuration for <see cref="InventorySite"/></summary>
public class InventorySiteSchema : IEntityTypeConfiguration<InventorySite>
{
    public void Configure(EntityTypeBuilder<InventorySite> builder)
    {
        builder.ToTable("inv_sites");

        builder.HasKey(e => e.Id).HasName("pk_inv_sites_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.Ignore(e => e.DeletedAt);

        builder.HasMany(e => e.Zones)
            .WithOne(e => e.Site)
            .HasForeignKey(e => e.SiteId)
            .HasConstraintName("fk_site_zones_site_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Lots)
            .WithOne(e => e.Site)
            .HasForeignKey(e => e.SiteId)
            .HasConstraintName("fk_lots_site_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.IntakeShipments)
            .WithOne(e => e.Site)
            .HasForeignKey(e => e.SiteId)
            .HasConstraintName("fk_intake_shipments_site_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Movements)
            .WithOne(e => e.Site)
            .HasForeignKey(e => e.SiteId)
            .HasConstraintName("fk_movements_site_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
