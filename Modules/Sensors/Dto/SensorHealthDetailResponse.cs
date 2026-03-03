using Shared.Enums;

namespace Modules.Sensors.Dto;

/// <summary>
/// Response for GET /api/v1/sensors/{id}/health (detailed health for one sensor).
/// </summary>
public class SensorHealthDetailResponse
{
    /// <summary>Sensor ID.</summary>
    public Guid SensorId { get; set; }

    /// <summary>Sensor name (serial/identifier).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Computed health status.</summary>
    public SensorHealthStatus HealthStatus { get; set; }

    /// <summary>Last time the sensor reported a reading (ISO 8601).</summary>
    public DateTime? LastSeenAt { get; set; }

    /// <summary>Reliability score 0–100.</summary>
    public double ReliabilityScore { get; set; }

    /// <summary>Most recent reading value.</summary>
    public double? LastValue { get; set; }

    /// <summary>Unit of last value (e.g. Celsius).</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>Seconds since last reading (freshness).</summary>
    public double? FreshnessSeconds { get; set; }

    /// <summary>Number of readings in the recent window (e.g. last 24h).</summary>
    public int RecentReadingsCount { get; set; }

    /// <summary>Expected number of readings in the window (for expected vs actual).</summary>
    public int ExpectedPoints { get; set; }

    /// <summary>Actual number of readings received in the window.</summary>
    public int ReceivedPoints { get; set; }
}
