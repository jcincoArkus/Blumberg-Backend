---
name: service-pattern
description: Pattern for creating business logic services with OpenTelemetry spans, logging, and entity returns in ASP.NET Core. Use when implementing business logic.
---

# Service Pattern

Create services at `Modules/{Feature}/Service/I{Feature}Service.cs` and `{Feature}Service.cs`.

Services return **entities**, not DTOs. The controller handles DTO mapping — this keeps services reusable across controllers, background jobs, etc.

**All list operations must be paginated** — never return unbounded lists.

## Interface Template

```csharp
using Shared.Dto;
using Modules.Items.Dto;

namespace Modules.Items.Service;

public interface IItemService
{
    Task<(IReadOnlyList<Shared.Entity.Item> Items, int TotalCount)> GetAllAsync(PaginationRequest request);
    /// <exception cref="KeyNotFoundException">When item is not found</exception>
    Task<Shared.Entity.Item> GetByIdAsync(Guid id);
    /// <exception cref="InvalidOperationException">When creation fails</exception>
    Task<Shared.Entity.Item> CreateAsync(ItemRequest request);
    /// <exception cref="KeyNotFoundException">When item is not found</exception>
    Task<Shared.Entity.Item> UpdateAsync(Guid id, ItemRequest request);
    /// <exception cref="KeyNotFoundException">When item is not found</exception>
    Task DeleteAsync(Guid id);
}
```

## Implementation Template

```csharp
using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Shared.Dto;
using Modules.Items.Dto;
using Modules.Items.Repository;

namespace Modules.Items.Service;

public class ItemService(
    IItemRepository itemRepository,
    ILogger<ItemService> logger) : IItemService
{
    [Span]
    public virtual async Task<(IReadOnlyList<Shared.Entity.Item> Items, int TotalCount)> GetAllAsync(
        PaginationRequest request)
    {
        logger.LogDebug("Getting items page {Page}, size {PageSize}", request.Page, request.PageSize);
        var result = await itemRepository.GetPagedAsync(request);
        logger.LogInformation("Retrieved {Count} of {Total} items", result.Items.Count, result.TotalCount);
        return result;
    }

    [Span]
    public virtual async Task<Shared.Entity.Item> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Getting item by ID: {Id}", id);
        var item = await itemRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Item with ID {id} was not found");
        logger.LogInformation("Retrieved item {Id}", id);
        return item;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<Shared.Entity.Item> CreateAsync(ItemRequest request)
    {
        logger.LogDebug("Creating new item: {Name}", request.Name);

        var item = new Shared.Entity.Item
        {
            Name = request.Name,
            Description = request.Description,
            // Map all request fields — do NOT set Id/CreatedAt/UpdatedAt (repo handles those)
        };

        var created = await itemRepository.CreateAsync(item);
        logger.LogInformation("Item created with ID: {Id}", created.Id);
        return created;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<Shared.Entity.Item> UpdateAsync(Guid id, ItemRequest request)
    {
        logger.LogDebug("Updating item {Id}", id);

        var item = await itemRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Item with ID {id} was not found");

        item.Name = request.Name;
        item.Description = request.Description;

        var updated = await itemRepository.UpdateAsync(item);
        logger.LogInformation("Item {Id} updated", id);
        return updated;
    }

    [Span]
    public virtual async Task DeleteAsync(Guid id)
    {
        logger.LogDebug("Deleting item {Id}", id);
        var deleted = await itemRepository.SoftDeleteAsync(id);
        if (!deleted)
            throw new KeyNotFoundException($"Item with ID {id} was not found");
        logger.LogInformation("Item {Id} deleted", id);
    }
}
```

## Project Conventions

- **Return entities, not DTOs** — controllers handle DTO mapping so services stay reusable
- **Always paginated** — list methods accept `PaginationRequest` and return `(IReadOnlyList<T> Items, int TotalCount)` tuple
- **Pass the query DTO through** — service receives `PaginationRequest` (or subclass) and passes it to the repository
- **`[Span]`** on all public methods — `[Span(IncludeArguments = true)]` for create/update
- **`virtual`** required on all public methods (Castle.DynamicProxy interception)
- **Throw exceptions** for error cases — controller handles HTTP mapping
- **Don't set timestamps**: `Id`, `CreatedAt`, `UpdatedAt` are set by the repository
- **Don't set `OrganizationId`**: `SaveChangesAsync` auto-sets it from `ITenantContext`
- **Logging**: Debug on entry, Info on success, Warning on not-found (before throw)
