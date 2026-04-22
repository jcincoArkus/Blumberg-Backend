using Shared.Enums;

namespace Modules.Sensors.Dto;

/// <summary>
/// Result type for sensor health list (service layer). Mapped to <see cref="SensorHealthListItemResponse"/> in the controller.
/// </summary>
public class SensorHealthListResult
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SensorType { get; set; } = string.Empty;
    public SensorHealthStatus HealthStatus { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public double ReliabilityScore { get; set; }
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public Guid EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public double? LastValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public IngestionSource? IngestionSource { get; set; }
}
