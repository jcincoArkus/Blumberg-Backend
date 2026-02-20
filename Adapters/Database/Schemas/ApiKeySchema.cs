using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>
/// Entity configuration for <see cref="ApiKey"/>
/// </summary>
public class ApiKeySchema : IEntityTypeConfiguration<ApiKey>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");

        builder.HasKey(e => e.Id).HasName("pk_api_keys_id");
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.KeyHash).HasColumnName("key_hash").IsRequired().HasMaxLength(64);
        builder.Property(e => e.OrganizationId).HasColumnName("organization_id").IsRequired();
        builder.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.RevokedAt).HasColumnName("revoked_at");

        builder.HasIndex(e => e.KeyHash).IsUnique().HasDatabaseName("ix_api_keys_key_hash");
        builder.HasIndex(e => e.OrganizationId).HasDatabaseName("ix_api_keys_organization_id");

        builder.HasOne(e => e.Organization)
            .WithMany()
            .HasForeignKey(e => e.OrganizationId)
            .HasConstraintName("fk_api_keys_organization_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
