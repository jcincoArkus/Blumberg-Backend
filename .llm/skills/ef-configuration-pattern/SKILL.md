---
name: ef-configuration-pattern
description: Pattern for configuring EF Core entities including DbSet registration, global query filters (soft delete + multi-tenant), enum-to-string storage, JSONB columns, and running migrations. Use when registering a new entity in the database or modifying schema.
---

# EF Configuration Pattern

All database configuration lives in `Adapters/Database/`.

## 1. Add DbSet in ApplicationDbContext

```csharp
// In ApplicationDbContext class
public DbSet<Item> Items => Set<Item>();
```

## 2. Add Global Query Filters in OnModelCreating

**Multi-tenant entity** (has `OrganizationId`):

```csharp
modelBuilder.Entity<Item>()
    .HasQueryFilter(e => e.DeletedAt == null
        && _tenantContext.CurrentOrganizationId != null
        && e.OrganizationId == _tenantContext.CurrentOrganizationId);
```

**Non-tenant entity** (soft delete only):

```csharp
modelBuilder.Entity<SensorType>().HasQueryFilter(e => e.DeletedAt == null);
```

## 3. Register New Enums

Add new enum types to the `domainEnumTypes` array in `OnModelCreating`. This stores them as `VARCHAR(50)` strings — allows adding enum values without migrations.

```csharp
var domainEnumTypes = new[]
{
    typeof(SensorStatus),
    typeof(SensorTypeKind),
    typeof(Unit),
    typeof(IngestionSource),
    typeof(IngestionStatus),
    typeof(SensorHealthStatus),
    typeof(NewEnumType),  // <-- add here
};
```

## 4. Entity Type Configuration (optional)

For entities needing custom table names, column names, indexes, or JSONB:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entity;

namespace Adapters.Database.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("items");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(i => i.Name).IsUnique();

        // JSONB column
        builder.Property(i => i.Metadata)
            .HasColumnType("jsonb");
    }
}
```

Configurations are auto-discovered via `modelBuilder.ApplyConfigurationsFromAssembly()` — no manual registration needed.

## 5. Multi-Tenant SaveChangesAsync

`ApplicationDbContext.SaveChangesAsync` automatically:
- Auto-sets `OrganizationId` from `ITenantContext` if entity has `Guid.Empty` org ID
- Prevents cross-tenant writes (throws `InvalidOperationException`)
- Allows explicit `OrganizationId` for seeders when no tenant context exists

## Migrations

Prefer the project CLI from backend root: `scripts/cli migration:generate <MigrationName>` and `scripts/cli migration:up`. See **migrations-pattern** skill for full workflow (including why each migration needs a .Designer.cs).

## Conventions

- EF global query filters handle both soft-delete and tenant filtering automatically
- Repositories also filter `DeletedAt == null` defensively (belt-and-suspenders)
- Enums are always `VARCHAR(50)` strings — register in `domainEnumTypes` array
- Configuration files go in `Adapters/Database/Configurations/`
- JSONB columns: use `.HasColumnType("jsonb")` in configuration
