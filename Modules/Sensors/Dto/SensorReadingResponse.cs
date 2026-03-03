using Shared.Enums;

namespace Modules.Sensors.Dto;

/// <summary>
/// Sensor reading response DTO (flat, no nested entities)
/// </summary>
public class SensorReadingResponse
{
    /// <summary>Reading ID</summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid Id { get; set; }

    /// <summary>Sensor ID</summary>
    /// <example>b2c3d4e5-f6a7-8901-bcde-f12345678901</example>
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

    /// <summary>Organization ID</summary>
    /// <example>c3d4e5f6-a7b8-9012-cdef-123456789012</example>
    public Guid OrganizationId { get; set; }

    /// <summary>Optional ingestion run ID</summary>
    /// <example>d4e5f6a7-b890-1234-defg-234567890123</example>
    public Guid? IngestionRunId { get; set; }

    /// <summary>Created at</summary>
    /// <example>2025-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }
}
