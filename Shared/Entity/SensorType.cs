namespace Shared.Entity;

/// <summary>
/// Sensor type entity representing a type of sensor
/// </summary>
public class SensorType : BaseEntity
{
    /// <summary>
    /// Sensor type's name
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Sensor type's unit of measurement
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Sensors classified by this type
    /// </summary>
    public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
}