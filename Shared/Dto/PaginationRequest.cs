namespace Shared.Dto;

/// <summary>
/// Base query parameters for paginated list endpoints.
/// Extend this class to add module-specific filters.
/// Invalid values are silently clamped to defaults.
/// </summary>
public class PaginationRequest
{
    private int _page = 1;
    private int _pageSize = 20;

    /// <summary>
    /// Search term to filter results
    /// </summary>
    /// <example>sensor-01</example>
    public string? Search { get; set; }

    /// <summary>
    /// Page number (1-based). Values below 1 default to 1.
    /// </summary>
    /// <example>1</example>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Number of items per page. Values outside 1–100 are clamped.
    /// </summary>
    /// <example>20</example>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 20 : value > 100 ? 100 : value;
    }
}
