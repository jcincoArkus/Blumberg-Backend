namespace Modules.Equipment.Dto;

/// <summary>
/// Paginated response wrapper
/// </summary>
/// <typeparam name="T">Item type</typeparam>
public class PagedResponse<T>
{
    /// <summary>Page items</summary>
    /// <example>[]</example>
    public IReadOnlyList<T> Items { get; set; } = [];

    /// <summary>Total count of items matching the query (before paging)</summary>
    /// <example>42</example>
    public int TotalCount { get; set; }

    /// <summary>Current page (1-based)</summary>
    /// <example>1</example>
    public int Page { get; set; }

    /// <summary>Page size</summary>
    /// <example>20</example>
    public int PageSize { get; set; }

    /// <summary>Total number of pages</summary>
    /// <example>3</example>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}
