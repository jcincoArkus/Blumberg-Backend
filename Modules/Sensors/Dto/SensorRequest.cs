using Shared.Enums;

namespace Modules.Sensors.Dto;

/// <summary>
/// Sensor request DTO
/// </summary>
public class SensorRequest
{
    /// <summary>
    /// Sensor serial number
    /// </summary>
    public string Serial { get; set; } = string.Empty;

    /// <summary>
    /// Sensor operational status
    /// </summary>
    public SensorStatus Status { get; set; }

    /// <summary>
    /// Equipment ID this sensor is contained in
    /// </summary>
    public Guid EquipmentId { get; set; }

    /// <summary>
    /// Sensor type ID that classifies this sensor
    /// </summary>
    public Guid SensorTypeId { get; set; }

    /// <summary>
    /// Threshold ID that configures this sensor
    /// </summary>
    public Guid ThresholdId { get; set; }
}
