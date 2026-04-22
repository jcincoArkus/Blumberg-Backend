using Shared.Dto;

namespace Modules.Sensors.Dto;

/// <summary>
/// Query parameters for sensor readings endpoint.
/// Extends PaginationRequest with time-range filters.
/// </summary>
public class GetSensorReadingsRequest : PaginationRequest
{
    /// <summary>
    /// Optional start of time range (UTC, inclusive)
    /// </summary>
    /// <example>2025-01-01T00:00:00Z</example>
    public DateTime? From { get; set; }

    /// <summary>
    /// Optional end of time range (UTC, inclusive)
    /// </summary>
    /// <example>2025-01-31T23:59:59Z</example>
    public DateTime? To { get; set; }
}
