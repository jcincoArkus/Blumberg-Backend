using Shared.Dto;
using Shared.Enums;

namespace Modules.Sensors.Dto;

/// <summary>
/// Request for GET /api/v1/sensors/health (paginated list with filters).
/// </summary>
public class GetSensorHealthRequest : PaginationRequest
{
    /// <summary>Filter by site ID.</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Filter by equipment ID.</summary>
    public Guid? EquipmentId { get; set; }

    /// <summary>Filter by sensor operational status.</summary>
    public SensorStatus? Status { get; set; }

    /// <summary>Filter by computed health status (Healthy, Warning, Critical, Stale, Silent, Offline).</summary>
    public SensorHealthStatus? HealthStatus { get; set; }
}
