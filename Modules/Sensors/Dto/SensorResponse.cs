using Shared.Enums;

namespace Modules.Sensors.Dto;

/// <summary>
/// Sensor response DTO (no nested entities to avoid JSON cycles)
/// </summary>
public class SensorResponse
{
    /// <summary>
    /// Sensor ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Sensor serial number
    /// </summary>
    public string Serial { get; set; } = string.Empty;

    /// <summary>
    /// Sensor operational status
    /// </summary>
    public SensorStatus Status { get; set; }

    /// <summary>
    /// Organization ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Organization name (for display)
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Equipment ID
    /// </summary>
    public Guid EquipmentId { get; set; }

    /// <summary>
    /// Equipment name (for display)
    /// </summary>
    public string EquipmentName { get; set; } = string.Empty;

    /// <summary>
    /// Sensor type ID
    /// </summary>
    public Guid SensorTypeId { get; set; }

    /// <summary>
    /// Sensor type name (for display)
    /// </summary>
    public string SensorTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Threshold ID
    /// </summary>
    public Guid ThresholdId { get; set; }

    /// <summary>
    /// Created at
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Updated at
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
