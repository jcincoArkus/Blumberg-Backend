---
name: service-pattern
description: Pattern for creating business logic services with OpenTelemetry spans, logging, and DTO mapping in ASP.NET Core. Use when implementing business logic.
---

# Service Pattern

## Overview

Pattern for creating **business logic services** in **ASP.NET Core** with automatic OpenTelemetry instrumentation via `[Span]` attribute, structured logging, and DTO mapping.

## Key Principles

1. **Interface + Implementation**: Always define interface for testability
2. **Span Attribute**: Use `[Span]` on public methods for tracing
3. **Virtual Methods**: Required for Castle.DynamicProxy interception
4. **Logging**: Debug on entry, Info on success, Warning on not found
5. **DTO Mapping**: Convert entities to DTOs to avoid circular references

## Template: Service Interface

```csharp
using Modules.Items.Dto;

namespace Modules.Items.Service;

/// <summary>
/// Service interface for item operations
/// </summary>
public interface IItemService
{
    /// <summary>
    /// Gets all items
    /// </summary>
    Task<List<ItemResponse>> GetAllAsync();

    /// <summary>
    /// Gets an item by ID
    /// </summary>
    /// <exception cref="KeyNotFoundException">When item is not found</exception>
    Task<ItemResponse> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new item
    /// </summary>
    /// <exception cref="InvalidOperationException">When creation fails</exception>
    Task<ItemResponse> CreateAsync(ItemRequest request);

    /// <summary>
    /// Updates an existing item
    /// </summary>
    /// <exception cref="KeyNotFoundException">When item is not found</exception>
    Task<ItemResponse> UpdateAsync(Guid id, ItemRequest request);

    /// <summary>
    /// Soft deletes an item
    /// </summary>
    /// <exception cref="KeyNotFoundException">When item is not found</exception>
    Task DeleteAsync(Guid id);
}
```

## Template: Service Implementation

```csharp
using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Items.Dto;
using Modules.Items.Repository;

namespace Modules.Items.Service;

/// <summary>
/// Service implementation for item operations
/// </summary>
public class ItemService(
    IItemRepository itemRepository,
    ILogger<ItemService> logger) : IItemService
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<List<ItemResponse>> GetAllAsync()
    {
        logger.LogDebug("Getting all items from repository");

        var items = await itemRepository.GetAllAsync();

        logger.LogInformation("Retrieved {Count} items", items.Count);

        return items.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<ItemResponse> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Getting item by ID: {Id}", id);

        var item = await itemRepository.GetByIdAsync(id);

        if (item == null)
        {
            logger.LogWarning("Item not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Item with ID {id} was not found");
        }

        logger.LogInformation("Retrieved item {Id}", id);

        return MapToResponse(item);
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<ItemResponse> CreateAsync(ItemRequest request)
    {
        logger.LogDebug("Creating new item: {Name}", request.Name);

        var item = new Shared.Entity.Item
        {
            Name = request.Name,
            Description = request.Description,
            Status = request.Status
        };

        var created = await itemRepository.CreateAsync(item);

        logger.LogInformation("Item created with ID: {Id}", created.Id);

        return MapToResponse(created);
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<ItemResponse> UpdateAsync(Guid id, ItemRequest request)
    {
        logger.LogDebug("Updating item {Id}", id);

        var item = await itemRepository.GetByIdAsync(id);

        if (item == null)
        {
            logger.LogWarning("Item not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Item with ID {id} was not found");
        }

        item.Name = request.Name;
        item.Description = request.Description;
        item.Status = request.Status;

        var updated = await itemRepository.UpdateAsync(item);

        logger.LogInformation("Item {Id} updated", id);

        return MapToResponse(updated);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task DeleteAsync(Guid id)
    {
        logger.LogDebug("Deleting item {Id}", id);

        var deleted = await itemRepository.SoftDeleteAsync(id);

        if (!deleted)
        {
            logger.LogWarning("Item not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Item with ID {id} was not found");
        }

        logger.LogInformation("Item {Id} deleted", id);
    }

    /// <summary>
    /// Maps entity to response DTO
    /// </summary>
    private static ItemResponse MapToResponse(Shared.Entity.Item item)
    {
        return new ItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Status = item.Status,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}
```

## Span Attribute Options

```csharp
// Basic span (default name: ClassName.MethodName)
[Span]
public virtual async Task<Item> GetByIdAsync(Guid id) { ... }

// Custom span name
[Span("GetItemById")]
public virtual async Task<Item> GetByIdAsync(Guid id) { ... }

// Include method arguments in span tags
[Span(IncludeArguments = true)]
public virtual async Task<Item> CreateAsync(ItemRequest request) { ... }

// Include return value in span tags
[Span(IncludeReturnValue = true)]
public virtual async Task<Item> GetByIdAsync(Guid id) { ... }

// Specify activity kind for external calls
[Span(Kind = ActivityKind.Client)]
public virtual async Task<Data> CallExternalApiAsync() { ... }

// Custom tags
[Span(Tags = "operation=read,entity=item")]
public virtual async Task<Item> GetByIdAsync(Guid id) { ... }
```

## Best Practices

✅ **DO:**
- Mark methods as `virtual` (required for proxy interception)
- Use `[Span]` on all public service methods
- Use `[Span(IncludeArguments = true)]` for create/update operations
- Throw specific exceptions (`KeyNotFoundException`, `InvalidOperationException`)
- Map entities to DTOs to avoid circular JSON references

❌ **DON'T:**
- Use non-virtual methods with `[Span]` (won't be intercepted)
- Return entities directly (use DTOs)
- Catch exceptions in service (let controller handle)
- Log sensitive data (passwords, tokens)
