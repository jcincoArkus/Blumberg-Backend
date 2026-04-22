using Shared.Enums;

namespace Shared.Entity;

/// <summary>
/// Sensor type entity representing a type of sensor
/// </summary>
public class SensorType : BaseEntity
{
    /// <summary>
    /// Kind/category of sensor (e.g. Temperature, Humidity)
    /// </summary>
    public SensorTypeKind Type { get; set; }

    /// <summary>
    /// Unit of measurement for this sensor type
    /// </summary>
    public Unit Unit { get; set; }

    /// <summary>
    /// Sensors classified by this type
    /// </summary>
    public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
}