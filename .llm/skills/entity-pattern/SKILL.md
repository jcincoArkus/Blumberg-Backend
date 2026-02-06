---
name: entity-pattern
description: Pattern for creating domain entities inheriting BaseEntity with multi-tenant OrganizationId, navigation properties, and enum usage. Use when adding a new database entity.
---

# Entity Pattern

Create entities at `Shared/Entity/{EntityName}.cs`. All entities inherit `BaseEntity`.

## BaseEntity (Shared/BaseEntity.cs)

Provides `Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt` — never redefine these.

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }  // null = active, set = soft-deleted
}
```

## Multi-Tenant Entity Template

Most entities are tenant-scoped. Include `OrganizationId` + navigation property.

```csharp
using Shared.Enums;

namespace Shared.Entity;

public class Item : BaseEntity
{
    // Domain fields
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ItemStatus Status { get; set; }

    // Tenant key (auto-set by SaveChangesAsync from ITenantContext)
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    // Foreign keys
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // Collections
    public ICollection<SubItem> SubItems { get; set; } = new List<SubItem>();
}
```

## Non-Tenant Entity Template

Some entities (e.g., `Organization`, `SensorType`, `Threshold`) are global — no `OrganizationId`.

```csharp
namespace Shared.Entity;

public class SensorType : BaseEntity
{
    public SensorTypeKind Type { get; set; }
    public Unit Unit { get; set; }
    public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
}
```

## Conventions

- **Namespace**: `Shared.Entity`
- **Enums**: Use types from `Shared.Enums/` — they're stored as `VARCHAR(50)` strings in DB
- **Navigation `= null!`**: Foreign key navigation properties use `= null!` (EF populates via `.Include()`)
- **Collections `= new List<T>()`**: Collection nav props initialized as empty lists
- **String defaults**: Required strings use `= string.Empty`; optional strings are `string?`
- **No logic in entities**: Entities are pure data classes (no methods except simple value objects)
- **OrganizationId**: Don't set manually in services — `SaveChangesAsync` handles it from `ITenantContext`

## After Creating an Entity

1. Register in `ApplicationDbContext` — add `DbSet<T>` and global query filters (see ef-configuration-pattern skill)
2. Add EF migration: `dotnet ef migrations add Add{Entity} --project Adapters/Database --startup-project Apps/API`
