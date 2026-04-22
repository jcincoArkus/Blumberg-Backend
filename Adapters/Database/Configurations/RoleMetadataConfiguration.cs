using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Configurations;

/// <summary>
/// Entity Framework configuration for RoleMetadata
/// </summary>
public class RoleMetadataConfiguration : IEntityTypeConfiguration<RoleMetadata>
{
    public void Configure(EntityTypeBuilder<RoleMetadata> builder)
    {
        builder.ToTable("role_metadata");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.Property(r => r.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(r => r.ColorR)
            .HasColumnName("color_r")
            .IsRequired();

        builder.Property(r => r.ColorG)
            .HasColumnName("color_g")
            .IsRequired();

        builder.Property(r => r.ColorB)
            .HasColumnName("color_b")
            .IsRequired();

        builder.Property(r => r.IsDefault)
            .HasColumnName("is_default")
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}

