using Shared.Enums;

namespace Modules.Sensors.Dto;

/// <summary>
/// Result type for sensor health detail (service layer). Mapped to <see cref="SensorHealthDetailResponse"/> in the controller.
/// </summary>
public class SensorHealthDetailResult
{
    public Guid SensorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public SensorHealthStatus HealthStatus { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public double ReliabilityScore { get; set; }
    public double? LastValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double? FreshnessSeconds { get; set; }
    public int RecentReadingsCount { get; set; }
    public int ExpectedPoints { get; set; }
    public int ReceivedPoints { get; set; }
}
