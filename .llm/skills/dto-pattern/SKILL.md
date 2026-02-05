---
name: dto-pattern
description: Pattern for creating request/response DTOs with validation attributes and documentation in ASP.NET Core. Use when defining API contracts.
---

# DTO Pattern

## Overview

Pattern for creating **Data Transfer Objects (DTOs)** for API request/response contracts with validation attributes, XML documentation, and pagination support.

## Key Principles

1. **Request DTOs**: Input data with validation attributes
2. **Response DTOs**: Output data with flattened relationships
3. **No Circular References**: Flatten nested entities to avoid JSON cycles
4. **Documentation**: XML comments for OpenAPI generation
5. **Pagination**: Use `PagedResponse<T>` for paginated endpoints

## Template: Request DTO

```csharp
using System.ComponentModel.DataAnnotations;
using Shared.Enums;

namespace Modules.Items.Dto;

/// <summary>
/// Request DTO for creating/updating an item
/// </summary>
public class ItemRequest
{
    /// <summary>
    /// Item name
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Item description (optional)
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Item status
    /// </summary>
    [Required(ErrorMessage = "Status is required")]
    public ItemStatus Status { get; set; }

    /// <summary>
    /// Category ID
    /// </summary>
    [Required(ErrorMessage = "CategoryId is required")]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Quantity (must be positive)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Email for notifications
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? NotificationEmail { get; set; }
}
```

## Template: Response DTO

```csharp
using Shared.Enums;

namespace Modules.Items.Dto;

/// <summary>
/// Response DTO for item data (flattened, no nested entities)
/// </summary>
public class ItemResponse
{
    /// <summary>
    /// Item ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Item name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Item description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Item status
    /// </summary>
    public ItemStatus Status { get; set; }

    /// <summary>
    /// Organization ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Organization name (flattened for display)
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Category ID
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Category name (flattened for display)
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
```

## Template: Paginated Response

```csharp
namespace Modules.Items.Dto;

/// <summary>
/// Generic paginated response wrapper
/// </summary>
/// <typeparam name="T">Item type</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// Page items
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = [];

    /// <summary>
    /// Total count of items matching the query (before paging)
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page (1-based)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling(TotalCount / (double)PageSize)
        : 0;
}
```

## Common Validation Attributes

| Attribute | Usage |
|-----------|-------|
| `[Required]` | Field must be provided |
| `[StringLength(max, MinimumLength = min)]` | String length constraints |
| `[Range(min, max)]` | Numeric range |
| `[EmailAddress]` | Email format validation |
| `[Phone]` | Phone number format |
| `[Url]` | URL format |
| `[RegularExpression(pattern)]` | Custom regex pattern |

## DTO Mapping in Service

```csharp
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
        OrganizationId = item.OrganizationId,
        OrganizationName = item.Organization?.Name ?? string.Empty,
        CategoryId = item.CategoryId,
        CategoryName = item.Category?.Name ?? string.Empty,
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt
    };
}
```

## Best Practices

✅ **DO:**
- Use validation attributes on request DTOs
- Flatten nested entities in response DTOs (use `Name` not `Entity`)
- Add XML documentation for OpenAPI generation
- Use nullable types (`string?`, `DateTime?`) for optional fields
- Use `= string.Empty` default for required strings

❌ **DON'T:**
- Include nested entity objects (causes JSON circular references)
- Expose internal IDs that shouldn't be public
- Skip validation attributes on required fields
- Use entity classes directly as DTOs
