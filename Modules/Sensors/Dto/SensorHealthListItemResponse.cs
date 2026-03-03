using Shared.Enums;

namespace Modules.Sensors.Dto;

/// <summary>
/// Single item in GET /api/v1/sensors/health list.
/// </summary>
public class SensorHealthListItemResponse
{
    /// <summary>Source of the last ingestion that produced a reading for this sensor (Api, Csv, or null if none).</summary>
    public IngestionSource? IngestionSource { get; set; }
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

    /// <summary>Site ID the sensor belongs to (via Equipment).</summary>
    public Guid SiteId { get; set; }

    /// <summary>Site name.</summary>
    public string SiteName { get; set; } = string.Empty;

    /// <summary>Equipment ID the sensor is attached to.</summary>
    public Guid EquipmentId { get; set; }

    /// <summary>Equipment name.</summary>
    public string EquipmentName { get; set; } = string.Empty;

    /// <summary>Most recent reading value.</summary>
    public double? LastValue { get; set; }

    /// <summary>Unit of the last reading (e.g. Celsius).</summary>
    public string Unit { get; set; } = string.Empty;
}
