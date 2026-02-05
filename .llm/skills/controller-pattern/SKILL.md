---
name: controller-pattern
description: Pattern for creating REST API controllers with error handling, authorization, OpenAPI documentation, and logging in ASP.NET Core. Use when creating API endpoints.
---

# Controller Pattern

## Overview

Pattern for creating **REST API controllers** in **ASP.NET Core** with proper error handling, authorization, OpenAPI documentation, and structured logging.

## Key Principles

1. **Route Convention**: `/api/v1/{resource}` with versioned endpoints
2. **Authorization**: `[Authorize]` attribute at controller level
3. **OpenAPI**: `[ProducesResponseType]` for all response types
4. **Error Handling**: Try-catch with specific exception handling
5. **Logging**: Debug on entry, Info on success, Warning/Error on failures

## Template: Controller

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Items.Dto;
using Modules.Items.Service;

namespace Modules.Items.Controller;

/// <summary>
/// Controller for managing items
/// </summary>
[ApiController]
[Route("api/v1/items")]
[Tags("Items")]
[Authorize]
public class ItemController(IItemService itemService, ILogger<ItemController> logger) : ControllerBase
{
    /// <summary>
    /// Gets all items
    /// </summary>
    [HttpGet(Name = "GetAllItemsV1")]
    [ProducesResponseType(typeof(List<ItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ItemResponse>>> GetAll()
    {
        logger.LogDebug("Getting all items");

        try
        {
            var items = await itemService.GetAllAsync();
            logger.LogInformation("Retrieved {Count} items", items.Count);
            return Ok(items);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all items");
            return StatusCode(500, new { message = "An error occurred while getting items" });
        }
    }

    /// <summary>
    /// Gets an item by ID
    /// </summary>
    [HttpGet("{id}", Name = "GetItemByIdV1")]
    [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemResponse>> GetById(Guid id)
    {
        logger.LogDebug("Getting item by ID: {Id}", id);

        try
        {
            var item = await itemService.GetByIdAsync(id);
            logger.LogInformation("Retrieved item {Id}", id);
            return Ok(item);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Item not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting item {Id}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Creates a new item
    /// </summary>
    [HttpPost(Name = "CreateItemV1")]
    [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ItemResponse>> Create([FromBody] ItemRequest request)
    {
        logger.LogDebug("Creating new item: {Name}", request.Name);

        try
        {
            var newItem = await itemService.CreateAsync(request);
            logger.LogInformation("Item created with ID: {Id}", newItem.Id);
            return CreatedAtAction(nameof(GetById), new { id = newItem.Id }, newItem);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Failed to create item: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating item");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Updates an item
    /// </summary>
    [HttpPut("{id}", Name = "UpdateItemV1")]
    [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemResponse>> Update(Guid id, [FromBody] ItemRequest request)
    {
        logger.LogDebug("Updating item {Id}", id);

        try
        {
            var updated = await itemService.UpdateAsync(id, request);
            logger.LogInformation("Item {Id} updated", id);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Item not found: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Failed to update item {Id}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating item {Id}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Soft deletes an item
    /// </summary>
    [HttpDelete("{id}", Name = "DeleteItemV1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        logger.LogDebug("Deleting item {Id}", id);

        try
        {
            await itemService.DeleteAsync(id);
            logger.LogInformation("Item {Id} deleted", id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Item not found: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting item {Id}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
```

## Template: Paginated Endpoint

```csharp
/// <summary>
/// Gets paginated items
/// </summary>
[HttpGet("paginated", Name = "GetItemsPaginatedV1")]
[ProducesResponseType(typeof(PagedResponse<ItemResponse>), StatusCodes.Status200OK)]
public async Task<ActionResult<PagedResponse<ItemResponse>>> GetPaginated(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? search = null)
{
    logger.LogDebug("Getting items page {Page}, size {PageSize}", page, pageSize);

    try
    {
        // Validate pagination params
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var result = await itemService.GetPaginatedAsync(page, pageSize, search);
        logger.LogInformation("Retrieved {Count} items", result.Items.Count);
        return Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error getting paginated items");
        return StatusCode(500, new { message = "An error occurred" });
    }
}
```

## Exception to HTTP Status Mapping

| Exception | HTTP Status | Log Level |
|-----------|-------------|-----------|
| `KeyNotFoundException` | 404 Not Found | Warning |
| `InvalidOperationException` | 400 Bad Request | Warning |
| `UnauthorizedAccessException` | 401/403 | Warning |
| `Exception` (generic) | 500 Internal Error | Error |

## Best Practices

✅ **DO:**
- Use primary constructor for DI: `(IService service, ILogger<T> logger)`
- Add `[ProducesResponseType]` for all possible responses
- Use `Name = "OperationV1"` for OpenAPI operation IDs
- Log at appropriate levels (Debug/Info/Warning/Error)
- Return `CreatedAtAction` for POST with location header

❌ **DON'T:**
- Put business logic in controllers (use services)
- Catch and swallow exceptions silently
- Return raw exception messages in production
- Forget to add `[Authorize]` for protected endpoints
