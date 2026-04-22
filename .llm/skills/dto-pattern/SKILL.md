---
name: dto-pattern
description: Pattern for creating request/response DTOs for API contracts in ASP.NET Core. Use when defining API contracts.
---

# DTO Pattern

Create DTOs at `Modules/{Feature}/Dto/`. Each module owns its own DTOs.

## Pagination Query DTO

`PaginationRequest` lives in `Shared/Dto/` as a shared base. Extend it in the module's Dto folder for custom filters.

```csharp
// Shared/Dto/PaginationRequest.cs (already exists)
namespace Shared.Dto;

public class PaginationRequest
{
    private int _page = 1;
    private int _pageSize = 20;

    /// <summary>Search term to filter results</summary>
    /// <example>sensor-01</example>
    public string? Search { get; set; }

    /// <summary>Page number (1-based). Values below 1 default to 1.</summary>
    /// <example>1</example>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>Number of items per page. Values outside 1–100 are clamped.</summary>
    /// <example>20</example>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 20 : value > 100 ? 100 : value;
    }
}
```

Extend for module-specific filters:

```csharp
// Modules/Items/Dto/GetItemReadingsRequest.cs
using Shared.Dto;

namespace Modules.Items.Dto;

/// <summary>Query parameters for item readings</summary>
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

## Request DTO (body)

```csharp
using System.ComponentModel.DataAnnotations;
using Shared.Enums;

namespace Modules.Items.Dto;

/// <summary>
/// Request body for creating or updating an item
/// </summary>
public class ItemRequest
{
    /// <summary>Display name of the item</summary>
    /// <example>Temperature Sensor A1</example>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description</summary>
    /// <example>Main floor sensor near entrance</example>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>Current operational status</summary>
    /// <example>Active</example>
    [Required(ErrorMessage = "Status is required")]
    public ItemStatus Status { get; set; }

    /// <summary>Category ID</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required(ErrorMessage = "CategoryId is required")]
    public Guid CategoryId { get; set; }
}
```

## Response DTO

Flatten navigation properties — use `{Entity}Name` instead of nested objects.

```csharp
using Shared.Enums;

namespace Modules.Items.Dto;

/// <summary>Item response with flattened navigation properties</summary>
public class ItemResponse
{
    /// <summary>Unique item identifier</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>Display name</summary>
    /// <example>Temperature Sensor A1</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description</summary>
    /// <example>Main floor sensor near entrance</example>
    public string? Description { get; set; }

    /// <summary>Current operational status</summary>
    /// <example>Active</example>
    public ItemStatus Status { get; set; }

    /// <summary>Organization ID</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid OrganizationId { get; set; }

    /// <summary>Organization name (flattened)</summary>
    /// <example>Acme Corp</example>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>Created timestamp (UTC)</summary>
    /// <example>2025-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>Last update timestamp (UTC)</summary>
    /// <example>2025-02-01T14:45:00Z</example>
    public DateTime? UpdatedAt { get; set; }
}
```

## PagedResponse

Each module that needs pagination defines its own `PagedResponse<T>` in its Dto folder.

```csharp
namespace Modules.Items.Dto;

/// <summary>Paginated response wrapper</summary>
public class PagedResponse<T>
{
    /// <summary>Items in the current page</summary>
    public IReadOnlyList<T> Items { get; set; } = [];
    /// <summary>Total items matching query</summary>
    /// <example>142</example>
    public int TotalCount { get; set; }
    /// <summary>Current page (1-based)</summary>
    /// <example>1</example>
    public int Page { get; set; }
    /// <summary>Page size</summary>
    /// <example>20</example>
    public int PageSize { get; set; }
    /// <summary>Total pages</summary>
    /// <example>8</example>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}
```

## Project Conventions

- **Query params as DTOs**: Use `[FromQuery] PaginationRequest` (or subclass) — not individual `[FromQuery]` params
- **`PaginationRequest` is shared**: Lives in `Shared/Dto/`; extend in module Dto folder for custom filters
- **Mapping in controller, not service**: Services return entities; controllers map to DTOs via `MapToResponse()`
- **OpenAPI docs on every property**: `/// <summary>` + `/// <example>` (see openapi-pattern skill for full guide)
- **Validation on request DTOs**: `[Required]`, `[StringLength]` with `ErrorMessage`
- **Pagination clamps, not errors**: `PaginationRequest` silently clamps invalid Page/PageSize to defaults via property setters (no `[Range]` attributes)
- **Flatten navigation properties**: `Organization.Name` -> `OrganizationName` (avoid JSON circular refs)
- **Nullable**: `string?` / `DateTime?` for optional; `= string.Empty` for required strings
- **Enums**: Reference from `Shared.Enums` — serialize as strings via EF config
