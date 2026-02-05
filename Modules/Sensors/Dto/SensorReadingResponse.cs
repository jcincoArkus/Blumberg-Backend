using Shared.Enums;

namespace Modules.Sensors.Dto;

/// <summary>
/// Sensor reading response DTO (flat, no nested entities)
/// </summary>
public class SensorReadingResponse
{
    /// <summary>Reading ID</summary>
    public Guid Id { get; set; }

    /// <summary>Sensor ID</summary>
    public Guid SensorId { get; set; }

    /// <summary>Reading value</summary>
    public decimal Value { get; set; }

    /// <summary>UTC timestamp when the reading was taken</summary>
    public DateTime TimestampUtc { get; set; }

    /// <summary>Unit of measurement</summary>
    public Unit Unit { get; set; }

    /// <summary>Organization ID</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Optional ingestion run ID</summary>
    public Guid? IngestionRunId { get; set; }

    /// <summary>Created at</summary>
    public DateTime CreatedAt { get; set; }
}
