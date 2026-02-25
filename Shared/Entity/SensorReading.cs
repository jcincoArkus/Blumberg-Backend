using Shared.Enums;

namespace Shared.Entity;

/// <summary>
/// Sensor reading entity representing a reading from a sensor
/// </summary>
public class SensorReading : BaseEntity
{
    /// <summary>
    /// Sensor reading's value
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// UTC timestamp when the reading was taken
    /// </summary>
    public DateTime TimestampUtc { get; set; }

    /// <summary>
    /// Unit of measurement for the value (stored as string)
    /// </summary>
    public Unit Unit { get; set; }

    /// <summary>
    /// Organization this reading belongs to
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Navigation to the organization
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// Sensor that produced this reading
    /// </summary>
    public Guid SensorId { get; set; }

    /// <summary>
    /// Navigation to the sensor
    /// </summary>
    public Sensor Sensor { get; set; } = null!;

    /// <summary>
    /// Optional ingestion run that produced this reading
    /// </summary>
    public Guid? IngestionRunId { get; set; }

    /// <summary>
    /// Navigation to the ingestion run (when set)
    /// </summary>
    public IngestionRun? IngestionRun { get; set; }

}