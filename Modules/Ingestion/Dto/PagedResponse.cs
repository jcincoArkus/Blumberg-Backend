namespace Modules.Ingestion.Dto;

/// <summary>
/// Paginated response wrapper for ingestion runs
/// </summary>
/// <typeparam name="T">Item type</typeparam>
public class PagedResponse<T>
{
    /// <summary>Page items</summary>
    public IReadOnlyList<T> Items { get; set; } = [];

    /// <summary>Total count of items matching the query (before paging)</summary>
    public int TotalCount { get; set; }

    /// <summary>Current page (1-based)</summary>
    public int Page { get; set; }

    /// <summary>Page size</summary>
    public int PageSize { get; set; }

    /// <summary>Total number of pages</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}
