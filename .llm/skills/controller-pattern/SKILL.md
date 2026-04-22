---
name: controller-pattern
description: Pattern for creating REST API controllers with error handling, authorization, OpenAPI documentation, DTO mapping, and logging in ASP.NET Core. Use when creating API endpoints.
---

# Controller Pattern

Create controllers at `Modules/{Feature}/Controller/{Feature}Controller.cs`.

Controllers receive **entities** from services and **map them to DTOs**. This keeps services reusable.

**All list endpoints must be paginated** — never return unbounded lists.

## Template

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Dto;
using Modules.Items.Dto;
using Modules.Items.Service;

namespace Modules.Items.Controller;

[ApiController]
[Route("api/v1/items")]
[Tags("Items")]
[Authorize]
public class ItemController(IItemService itemService, ILogger<ItemController> logger) : ControllerBase
{
    [HttpGet(Name = "GetItemsV1")]
    [ProducesResponseType(typeof(PagedResponse<ItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ItemResponse>>> GetAll([FromQuery] PaginationRequest request)
    {
        logger.LogDebug("Getting items page {Page}, size {PageSize}", request.Page, request.PageSize);
        try
        {
            var (items, totalCount) = await itemService.GetAllAsync(request);
            logger.LogInformation("Retrieved {Count} items (total {Total})", items.Count, totalCount);

            return Ok(new PagedResponse<ItemResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting items");
            return StatusCode(500, new { message = "An error occurred while getting items" });
        }
    }

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
            return Ok(MapToResponse(item));
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

    [HttpPost(Name = "CreateItemV1")]
    [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ItemResponse>> Create([FromBody] ItemRequest request)
    {
        logger.LogDebug("Creating new item: {Name}", request.Name);
        try
        {
            var item = await itemService.CreateAsync(request);
            logger.LogInformation("Item created with ID: {Id}", item.Id);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToResponse(item));
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

    [HttpPut("{id}", Name = "UpdateItemV1")]
    [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemResponse>> Update(Guid id, [FromBody] ItemRequest request)
    {
        logger.LogDebug("Updating item {Id}", id);
        try
        {
            var item = await itemService.UpdateAsync(id, request);
            logger.LogInformation("Item {Id} updated", id);
            return Ok(MapToResponse(item));
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

    // DTO mapping — lives in the controller, not the service
    private static ItemResponse MapToResponse(Shared.Entity.Item item)
    {
        return new ItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            OrganizationId = item.OrganizationId,
            OrganizationName = item.Organization?.Name ?? string.Empty,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}
```

## Nested Paginated Endpoint with Custom Query DTO

For child resources with extra filters, extend `PaginationRequest` in the module's Dto folder:

```csharp
// Modules/Items/Dto/GetItemReadingsRequest.cs
using Shared.Dto;

namespace Modules.Items.Dto;

/// <summary>Query parameters for sensor readings</summary>
public class GetItemReadingsRequest : PaginationRequest
{
    /// <summary>Start of time range (UTC, inclusive)</summary>
    /// <example>2025-01-01T00:00:00Z</example>
    public DateTime? From { get; set; }

    /// <summary>End of time range (UTC, inclusive)</summary>
    /// <example>2025-12-31T23:59:59Z</example>
    public DateTime? To { get; set; }
}
```

```csharp
[HttpGet("{id}/readings", Name = "GetItemReadingsV1")]
[ProducesResponseType(typeof(PagedResponse<ReadingResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<PagedResponse<ReadingResponse>>> GetReadings(
    Guid id, [FromQuery] GetItemReadingsRequest request)
{
    logger.LogDebug("Getting readings for item {Id}, page {Page}", id, request.Page);
    try
    {
        var (items, totalCount) = await itemService.GetReadingsAsync(id, request);
        logger.LogInformation("Retrieved {Count} readings for item {Id}", items.Count, id);

        return Ok(new PagedResponse<ReadingResponse>
        {
            Items = items.Select(MapReadingToResponse).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
    catch (KeyNotFoundException ex)
    {
        logger.LogWarning("Item not found with ID: {Id}", id);
        return NotFound(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error getting readings for item {Id}", id);
        return StatusCode(500, new { message = "An error occurred" });
    }
}
```

## Project Conventions

- **Always paginated**: Every list endpoint returns `PagedResponse<T>` — no unbounded lists
- **Query DTOs**: Use `[FromQuery] PaginationRequest` (or a subclass) instead of individual `[FromQuery]` params
- **Route**: `api/v1/{resource}` (plural, lowercase)
- **Operation IDs**: `Name = "VerbNounV1"` (e.g., `"GetItemsV1"`)
- **Auth**: `[Authorize]` at class level
- **No `[Span]`**: Controllers do NOT use `[Span]` — only services/repos do
- **DTO mapping in controller**: `MapToResponse()` private static method in controller, not service
- **PagedResponse wrapping**: Controller builds `PagedResponse<TDto>` from service's `(Items, TotalCount)` tuple
- **Exception mapping**: `KeyNotFoundException` -> 404, `InvalidOperationException` -> 400, `Exception` -> 500
- **Logging**: Debug on entry, Info on success, Warning on client error, Error on server error
