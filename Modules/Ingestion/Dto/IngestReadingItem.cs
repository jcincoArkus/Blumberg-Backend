using Shared.Enums;

namespace Modules.Ingestion.Dto;

/// <summary>
/// A single reading in a batch ingestion request
/// </summary>
public class IngestReadingItem
{
    /// <summary>Sensor ID</summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid SensorId { get; set; }

    /// <summary>Reading value</summary>
    /// <example>72.5</example>
    public decimal Value { get; set; }

    /// <summary>UTC timestamp when the reading was taken</summary>
    /// <example>2025-01-15T10:30:00Z</example>
    public DateTime TimestampUtc { get; set; }

    /// <summary>Unit of measurement</summary>
    /// <example>Fahrenheit</example>
    public Unit Unit { get; set; }
}
