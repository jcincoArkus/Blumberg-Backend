---
name: repository-pattern
description: Pattern for creating EF Core repositories with soft delete, tenant filtering, eager loading, and OpenTelemetry spans. Use when implementing data access.
---

# Repository Pattern

## Overview

Pattern for creating **EF Core repositories** with soft delete support, multi-tenant filtering, eager loading via `.Include()`, and automatic OpenTelemetry instrumentation.

## Key Principles

1. **Interface + Implementation**: Always define interface for testability
2. **Soft Delete**: Use `DeletedAt` field instead of hard deletes
3. **Eager Loading**: Include related entities with `.Include()`
4. **Span Attribute**: Use `[Span]` on all public methods
5. **Virtual Methods**: Required for Castle.DynamicProxy interception

## Template: Repository Interface

```csharp
namespace Modules.Items.Repository;

/// <summary>
/// Repository interface for Item entity operations
/// </summary>
public interface IItemRepository
{
    /// <summary>
    /// Gets all items that are not soft deleted
    /// </summary>
    Task<List<Shared.Entity.Item>> GetAllAsync();

    /// <summary>
    /// Gets an item by ID
    /// </summary>
    /// <returns>Item entity or null if not found</returns>
    Task<Shared.Entity.Item?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new item
    /// </summary>
    Task<Shared.Entity.Item> CreateAsync(Shared.Entity.Item item);

    /// <summary>
    /// Updates an existing item
    /// </summary>
    Task<Shared.Entity.Item> UpdateAsync(Shared.Entity.Item item);

    /// <summary>
    /// Soft deletes an item by setting DeletedAt timestamp
    /// </summary>
    /// <returns>True if found and deleted, false otherwise</returns>
    Task<bool> SoftDeleteAsync(Guid id);
}
```

## Template: Repository Implementation

```csharp
using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Modules.Items.Repository;

/// <summary>
/// Repository implementation for Item entity operations
/// </summary>
public class ItemRepository(
    ApplicationDbContext context,
    ILogger<ItemRepository> logger) : IItemRepository
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc />
    [Span]
    public virtual async Task<List<Shared.Entity.Item>> GetAllAsync()
    {
        logger.LogDebug("Querying all items");

        var items = await _context.Items
            .Where(i => i.DeletedAt == null)
            .Include(i => i.Organization)
            .Include(i => i.Category)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} items from database", items.Count);

        return items;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Shared.Entity.Item?> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Querying item by ID: {Id}", id);

        var item = await _context.Items
            .Include(i => i.Organization)
            .Include(i => i.Category)
            .FirstOrDefaultAsync(i => i.Id == id && i.DeletedAt == null);

        if (item != null)
            logger.LogInformation("Found item {Id}", id);
        else
            logger.LogDebug("Item not found with ID: {Id}", id);

        return item;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Shared.Entity.Item> CreateAsync(Shared.Entity.Item item)
    {
        logger.LogDebug("Creating item in database: {Name}", item.Name);

        item.Id = Guid.NewGuid();
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = null;
        item.DeletedAt = null;

        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        logger.LogInformation("Item created with ID: {Id}", item.Id);

        return item;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Shared.Entity.Item> UpdateAsync(Shared.Entity.Item item)
    {
        logger.LogDebug("Updating item in database: {Id}", item.Id);

        item.UpdatedAt = DateTime.UtcNow;

        _context.Items.Update(item);
        await _context.SaveChangesAsync();

        logger.LogInformation("Item {Id} updated", item.Id);

        return item;
    }

    /// <inheritdoc />
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

## Template: Paginated Query

```csharp
/// <summary>
/// Gets paginated items
/// </summary>
[Span]
public virtual async Task<(List<Shared.Entity.Item> Items, int TotalCount)> GetPaginatedAsync(
    int page,
    int pageSize,
    string? search = null)
{
    logger.LogDebug("Querying items page {Page}, size {PageSize}", page, pageSize);

    var query = _context.Items
        .Where(i => i.DeletedAt == null);

    // Apply search filter
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(i => i.Name.Contains(search));
    }

    // Get total count before pagination
    var totalCount = await query.CountAsync();

    // Apply pagination
    var items = await query
        .Include(i => i.Organization)
        .OrderBy(i => i.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    logger.LogInformation("Retrieved {Count} of {Total} items", items.Count, totalCount);

    return (items, totalCount);
}
```

## Common Patterns

**Filter by related entity:**
```csharp
var items = await _context.Items
    .Where(i => i.CategoryId == categoryId && i.DeletedAt == null)
    .ToListAsync();
```

**Multiple includes:**
```csharp
var item = await _context.Items
    .Include(i => i.Organization)
    .Include(i => i.Category)
    .Include(i => i.Tags)
    .FirstOrDefaultAsync(i => i.Id == id);
```

**Nested includes:**
```csharp
var item = await _context.Items
    .Include(i => i.Category)
        .ThenInclude(c => c.ParentCategory)
    .FirstOrDefaultAsync(i => i.Id == id);
```

## Best Practices

✅ **DO:**
- Mark methods as `virtual` (required for proxy interception)
- Use `[Span]` on all public repository methods
- Always filter by `DeletedAt == null` for soft deletes
- Set `CreatedAt` and `UpdatedAt` timestamps
- Use `.Include()` for eager loading related entities

❌ **DON'T:**
- Hard delete records (use soft delete)
- Use `AsNoTracking()` when you need to update
- Forget to call `SaveChangesAsync()`
- Return IQueryable (materialize with `ToListAsync()`)
