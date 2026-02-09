using Shared.Enums;
using Shared.Dto;

namespace Modules.Ingestion.Dto;

/// <summary>
/// Query parameters for listing ingestion runs (paginated, filterable)
/// </summary>
public class GetIngestionRunsRequest : PaginationRequest
{
    /// <summary>Filter by status</summary>
    public IngestionStatus? Status { get; set; }

    /// <summary>Filter by source</summary>
    public IngestionSource? Source { get; set; }

    /// <summary>Start of date range (run CreatedAt or StartedAt), UTC inclusive</summary>
    /// <example>2025-01-01T00:00:00Z</example>
    public DateTime? From { get; set; }

    /// <summary>End of date range (run CreatedAt or CompletedAt), UTC inclusive</summary>
    /// <example>2025-01-31T23:59:59Z</example>
    public DateTime? To { get; set; }
}
