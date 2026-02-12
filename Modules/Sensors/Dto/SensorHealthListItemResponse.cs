using Shared.Enums;

namespace Modules.Sensors.Dto;

/// <summary>
/// Single item in GET /api/v1/sensors/health list.
/// </summary>
public class SensorHealthListItemResponse
{
    /// <summary>Sensor ID.</summary>
    public Guid Id { get; set; }

    /// <summary>Sensor name (serial/identifier).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Sensor type (e.g. Temperature).</summary>
    public string SensorType { get; set; } = string.Empty;

    /// <summary>Computed health status.</summary>
    public SensorHealthStatus HealthStatus { get; set; }

    /// <summary>Last time the sensor reported a reading (ISO 8601).</summary>
    public DateTime? LastSeenAt { get; set; }

    /// <summary>Reliability score 0–100.</summary>
    public double ReliabilityScore { get; set; }
}
