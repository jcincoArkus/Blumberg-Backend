---
name: repository-pattern
description: Pattern for creating EF Core repositories with soft delete, tenant filtering, eager loading, and OpenTelemetry spans. Use when implementing data access.
---

# Repository Pattern

Create repositories at `Modules/{Feature}/Repository/I{Feature}Repository.cs` and `{Feature}Repository.cs`.

**All list queries must be paginated** — never return unbounded lists.

## Interface Template

```csharp
using Shared.Dto;

namespace Modules.Items.Repository;

public interface IItemRepository
{
    Task<(IReadOnlyList<Shared.Entity.Item> Items, int TotalCount)> GetPagedAsync(PaginationRequest request);
    Task<Shared.Entity.Item?> GetByIdAsync(Guid id);
    Task<Shared.Entity.Item> CreateAsync(Shared.Entity.Item item);
    Task<Shared.Entity.Item> UpdateAsync(Shared.Entity.Item item);
    Task<bool> SoftDeleteAsync(Guid id);
}
```

## Implementation Template

```csharp
using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Dto;

namespace Modules.Items.Repository;

public class ItemRepository(
    ApplicationDbContext context,
    ILogger<ItemRepository> logger) : IItemRepository
{
    private readonly ApplicationDbContext _context = context;

    [Span]
    public virtual async Task<(IReadOnlyList<Shared.Entity.Item> Items, int TotalCount)> GetPagedAsync(
        PaginationRequest request)
    {
        logger.LogDebug("Querying items page {Page}, size {PageSize}", request.Page, request.PageSize);

        var query = _context.Items.Where(i => i.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(i => i.Name.Contains(request.Search));

        var totalCount = await query.CountAsync();

        var items = await query
            .Include(i => i.Organization)
            .OrderBy(i => i.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} of {Total} items from database", items.Count, totalCount);
        return (items, totalCount);
    }

    [Span]
    public virtual async Task<Shared.Entity.Item?> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Querying item by ID: {Id}", id);

        var item = await _context.Items
            .Include(i => i.Organization)
            .FirstOrDefaultAsync(i => i.Id == id && i.DeletedAt == null);

        if (item != null)
            logger.LogInformation("Found item {Id}", id);
        else
            logger.LogDebug("Item not found with ID: {Id}", id);

        return item;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<Shared.Entity.Item> CreateAsync(Shared.Entity.Item item)
    {
        logger.LogDebug("Creating item in database");

        item.Id = Guid.NewGuid();
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = null;
        item.DeletedAt = null;

        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        logger.LogInformation("Item created with ID: {Id}", item.Id);
        return item;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<Shared.Entity.Item> UpdateAsync(Shared.Entity.Item item)
    {
        logger.LogDebug("Updating item in database: {Id}", item.Id);

        item.UpdatedAt = DateTime.UtcNow;

        _context.Items.Update(item);
        await _context.SaveChangesAsync();

        logger.LogInformation("Item {Id} updated in database", item.Id);
        return item;
    }

    [Span]
    public virtual async Task<bool> SoftDeleteAsync(Guid id)
    {
        logger.LogDebug("Soft deleting item: {Id}", id);

        var item = await GetByIdAsync(id);
        if (item == null)
        {
            logger.LogWarning("Item not found for deletion: {Id}", id);
            return false;
        }

        item.DeletedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        logger.LogInformation("Item {Id} soft deleted", id);
        return true;
    }
}
```

## Project Conventions

- **Always paginated** — list methods accept `PaginationRequest` and return `(IReadOnlyList<T> Items, int TotalCount)` tuple
- **`PaginationRequest`** — shared base class in `Shared/Dto/` with `Page`, `PageSize`, `Search`; extend for custom filters
- **`[Span]`** on all public methods — `[Span(IncludeArguments = true)]` for create/update
- **`virtual`** required on all public methods (Castle.DynamicProxy interception)
- **Timestamps**: Repository sets `Id = Guid.NewGuid()`, `CreatedAt = DateTime.UtcNow` on create; `UpdatedAt = DateTime.UtcNow` on update/delete
- **OrganizationId**: Do NOT set manually — `SaveChangesAsync` auto-sets it from `ITenantContext`
- **Soft-delete filter**: Always add `.Where(e => e.DeletedAt == null)` explicitly in queries. Global query filters exist in `ApplicationDbContext` but repos filter defensively as well.
- **Eager loading**: Use `.Include()` for navigation properties needed by the controller's `MapToResponse()`
- **Count before pagination**: Always `CountAsync()` before `.Skip().Take()` to get total
