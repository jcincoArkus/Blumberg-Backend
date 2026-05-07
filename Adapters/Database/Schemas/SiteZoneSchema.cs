using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>Entity configuration for <see cref="SiteZone"/></summary>
public class SiteZoneSchema : IEntityTypeConfiguration<SiteZone>
{
    public void Configure(EntityTypeBuilder<SiteZone> builder)
    {
        builder.ToTable("site_zones");

        builder.HasKey(e => e.Id).HasName("pk_site_zones_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(e => e.SiteId).HasColumnName("site_id").IsRequired();
        builder.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

        // No updated_at or deleted_at columns in this table
        builder.Ignore(e => e.UpdatedAt);
        builder.Ignore(e => e.DeletedAt);

        builder.HasIndex(e => e.SiteId).HasDatabaseName("idx_site_zones_site_id");
        builder.HasIndex(e => new { e.SiteId, e.Name }).IsUnique();

        builder.HasOne(e => e.Site)
            .WithMany(e => e.Zones)
            .HasForeignKey(e => e.SiteId)
            .HasConstraintName("fk_site_zones_site_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
