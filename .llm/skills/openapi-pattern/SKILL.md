---
name: openapi-pattern
description: Pattern for rich OpenAPI/Swagger documentation on DTOs and controllers using Swashbuckle in ASP.NET Core. Covers XML summaries, validation attributes, examples, descriptions, and ProducesResponseType. Use when creating or improving API documentation on DTOs and endpoints.
---

# OpenAPI Pattern

All DTOs and controller endpoints must have complete OpenAPI documentation for Swashbuckle (v10.1+) to generate a high-quality spec.

## Request DTO — Full Documentation

Every property needs: XML summary, validation attribute(s), and example value.

```csharp
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Shared.Enums;

namespace Modules.Items.Dto;

/// <summary>
/// Request body for creating or updating an item
/// </summary>
public class ItemRequest
{
    /// <summary>
    /// Display name of the item
    /// </summary>
    /// <example>Temperature Sensor A1</example>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the item
    /// </summary>
    /// <example>Main floor temperature sensor near entrance</example>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Current operational status
    /// </summary>
    /// <example>Active</example>
    [Required(ErrorMessage = "Status is required")]
    public ItemStatus Status { get; set; }

    /// <summary>
    /// ID of the category this item belongs to
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required(ErrorMessage = "CategoryId is required")]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Quantity (must be at least 1)
    /// </summary>
    /// <example>5</example>
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Notification email address
    /// </summary>
    /// <example>admin@example.com</example>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? NotificationEmail { get; set; }

    /// <summary>
    /// Target threshold value
    /// </summary>
    /// <example>25.5</example>
    [Range(0.0, 1000.0, ErrorMessage = "Value must be between 0 and 1000")]
    public decimal? TargetValue { get; set; }

    /// <summary>
    /// Start date for the monitoring period
    /// </summary>
    /// <example>2025-01-15T00:00:00Z</example>
    public DateTime? StartDate { get; set; }
}
```

## Response DTO — Full Documentation

Every property needs: XML summary and example value.

```csharp
using Shared.Enums;

namespace Modules.Items.Dto;

/// <summary>
/// Item response with flattened navigation properties
/// </summary>
public class ItemResponse
{
    /// <summary>
    /// Unique item identifier
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Display name of the item
    /// </summary>
    /// <example>Temperature Sensor A1</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description
    /// </summary>
    /// <example>Main floor temperature sensor near entrance</example>
    public string? Description { get; set; }

    /// <summary>
    /// Current operational status
    /// </summary>
    /// <example>Active</example>
    public ItemStatus Status { get; set; }

    /// <summary>
    /// Organization ID
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Organization name
    /// </summary>
    /// <example>Acme Corp</example>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the item was created (UTC)
    /// </summary>
    /// <example>2025-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp of last update (UTC), null if never updated
    /// </summary>
    /// <example>2025-02-01T14:45:00Z</example>
    public DateTime? UpdatedAt { get; set; }
}
```

## PagedResponse — Full Documentation

```csharp
namespace Modules.Items.Dto;

/// <summary>
/// Paginated response wrapper
/// </summary>
/// <typeparam name="T">Type of items in the page</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// Items in the current page
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = [];

    /// <summary>
    /// Total number of items matching the query (before paging)
    /// </summary>
    /// <example>142</example>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    /// <example>1</example>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    /// <example>20</example>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    /// <example>8</example>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}
```

## Controller Endpoint Documentation

```csharp
/// <summary>
/// Creates a new item
/// </summary>
/// <param name="request">Item creation data</param>
/// <returns>The created item</returns>
/// <response code="201">Item created successfully</response>
/// <response code="400">Validation error or business rule violation</response>
[HttpPost(Name = "CreateItemV1")]
[ProducesResponseType(typeof(ItemResponse), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<ItemResponse>> Create([FromBody] ItemRequest request)
```

## Validation Attributes Quick Reference

| Type | Attributes | Example |
|------|-----------|---------|
| Required string | `[Required]` `[StringLength(max, MinimumLength = min)]` | Name, Email |
| Optional string | `[StringLength(max)]` | Description |
| Required Guid | `[Required]` | CategoryId |
| Email | `[Required]` `[EmailAddress]` | Email |
| Integer range | `[Range(min, max)]` | Quantity |
| Decimal range | `[Range(0.0, 1000.0)]` | Value |
| Enum | `[Required]` | Status |
| Phone | `[Phone]` | PhoneNumber |
| URL | `[Url]` | WebsiteUrl |
| Regex | `[RegularExpression(@"pattern")]` | SerialNumber |

## Checklist for Every DTO Property

1. `/// <summary>` — concise description of what the property represents
2. `/// <example>` — realistic example value (Swashbuckle uses this in the spec)
3. Validation attribute(s) — `[Required]`, `[StringLength]`, `[Range]`, etc. (request DTOs)
4. `ErrorMessage` — human-readable message on every validation attribute
