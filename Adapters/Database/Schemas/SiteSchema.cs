using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>
/// Entity configuration for <see cref="Site"/> entity
/// </summary>
/// <remarks>
/// Configures the Sites table with PostgreSQL snake_case naming conventions
/// </remarks>
public class SiteSchema : IEntityTypeConfiguration<Site>
{
    /// <summary>
    /// Configures the Site entity
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        // Table name
        builder.ToTable("sites");

        // Primary key
        builder.HasKey(e => e.Id).HasName("pk_sites_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

        // Properties
        builder.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(e => e.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.HasIndex(e => e.OrganizationId).HasDatabaseName("ix_sites_organization_id");
        builder.Property(e => e.Address).HasColumnName("address").IsRequired().HasMaxLength(255);
        builder.Property(e => e.City).HasColumnName("city").IsRequired().HasMaxLength(120);
        builder.Property(e => e.State).HasColumnName("state").IsRequired().HasMaxLength(120);
        builder.Property(e => e.PostalCode).HasColumnName("postal_code").IsRequired().HasMaxLength(40);
        builder.Property(e => e.Country).HasColumnName("country").IsRequired().HasMaxLength(120);

        // Timestamps
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(e => e.CreatedAt).IsDescending().HasDatabaseName("ix_sites_created_at_desc");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");

        // Relationships: Site belongs to Organization
        builder.HasOne(e => e.Organization)
            .WithMany(e => e.Sites)
            .HasForeignKey(e => e.OrganizationId)
            .HasConstraintName("fk_sites_organization_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Relationships: Site has many Equipment
        builder.HasMany(e => e.Equipment)
            .WithOne(e => e.Site)
            .HasForeignKey(e => e.SiteId)
            .HasConstraintName("fk_equipment_site_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

