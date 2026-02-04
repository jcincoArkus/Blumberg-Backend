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
    /// Alerts triggered by this sensor reading
    /// </summary>
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}