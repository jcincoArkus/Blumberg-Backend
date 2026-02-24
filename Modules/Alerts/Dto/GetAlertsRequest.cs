using Shared.Dto;
using Shared.Enums;

namespace Modules.Alerts.Dto;

/// <summary>
/// Request DTO for paginated alert queries with optional filters
/// </summary>
public class GetAlertsRequest : PaginationRequest
{
    /// <summary>Filter by alert status</summary>
    public AlertStatus? Status { get; set; }

    /// <summary>Filter by sensor ID</summary>
    public Guid? SensorId { get; set; }

    /// <summary>Filter by equipment ID</summary>
    public Guid? EquipmentId { get; set; }

    /// <summary>Filter by site ID</summary>
    public Guid? SiteId { get; set; }
}
