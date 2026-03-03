namespace Modules.Alerts.Dto;

/// <summary>
/// Paginated response wrapper
/// </summary>
public class PagedResponse<T>
{
    /// <summary>Page items</summary>
    public IReadOnlyList<T> Items { get; set; } = [];

    /// <summary>Total count matching the query</summary>
    public int TotalCount { get; set; }

    /// <summary>Current page (1-based)</summary>
    public int Page { get; set; }

    /// <summary>Page size</summary>
    public int PageSize { get; set; }

    /// <summary>Total pages</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}
