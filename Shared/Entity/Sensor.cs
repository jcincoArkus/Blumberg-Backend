using Shared.Enums;

namespace Shared.Entity;

/// <summary>
/// Sensor entity representing a sensor device
/// </summary>
public class Sensor : BaseEntity
{
    /// <summary>
    /// Sensor's serial number
    /// </summary>
    public string Serial { get; set; } = string.Empty;

    /// <summary>
    /// Sensor's operational status
    /// </summary>
    public SensorStatus Status { get; set; }

    /// <summary>
    /// Organization this sensor belongs to
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Navigation to the organization
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// Equipment this sensor is contained in
    /// </summary>
    public Guid EquipmentId { get; set; }

    /// <summary>
    /// Navigation to the equipment
    /// </summary>
    public Equipment Equipment { get; set; } = null!;

    /// <summary>
    /// Sensor type that classifies this sensor
    /// </summary>
    public Guid SensorTypeId { get; set; }

    /// <summary>
    /// Navigation to the sensor type
    /// </summary>
    public SensorType SensorType { get; set; } = null!;

    /// <summary>
    /// Threshold that configures this sensor
    /// </summary>
    public Guid ThresholdId { get; set; }

    /// <summary>
    /// Navigation to the threshold
    /// </summary>
    public Threshold Threshold { get; set; } = null!;

    /// <summary>
    /// Sensor readings produced by this sensor
    /// </summary>
    public ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
}