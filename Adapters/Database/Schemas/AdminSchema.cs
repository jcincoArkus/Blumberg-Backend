using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Schemas;

/// <summary>
/// Entity configuration for Admin entity
/// </summary>
/// <remarks>
/// Configures the Admin table with PostgreSQL snake_case naming conventions
/// </remarks>
public class AdminSchema : IEntityTypeConfiguration<Admin>
{
    /// <summary>
    /// Configures the Admin entity
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        // Table name
        builder.ToTable("admins");

        // Primary key
        builder.HasKey(e => e.Id).HasName("pk_admins_id");
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
        // Email (unique, required)
        builder.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
        builder.HasIndex(e => e.Email).IsUnique().HasDatabaseName("ix_admins_email_unique");
        // Password hash (required)
        builder.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
        // First name (required)
        builder.Property(e => e.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(100);
        // Last name (required)
        builder.Property(e => e.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(100);
        // Timestamps
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(e => e.CreatedAt).IsDescending().HasDatabaseName("ix_admins_created_at_desc");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Property(e => e.DeletedAt).HasColumnName("deleted_at");
    }
}

