using System.ComponentModel.DataAnnotations;
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
    /// <example>SN-001-2025</example>
    [Required]
    [StringLength(100)]
    public string Serial { get; set; } = string.Empty;

    /// <summary>
    /// Sensor operational status
    /// </summary>
    /// <example>Active</example>
    [Required]
    public SensorStatus Status { get; set; }

    /// <summary>
    /// Equipment ID this sensor is contained in
    /// </summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    [Required]
    public Guid EquipmentId { get; set; }

    /// <summary>
    /// Sensor type ID that classifies this sensor
    /// </summary>
    /// <example>b2c3d4e5-f6a7-8901-bcde-f12345678901</example>
    [Required]
    public Guid SensorTypeId { get; set; }

    /// <summary>
    /// Threshold ID that configures this sensor
    /// </summary>
    /// <example>c3d4e5f6-a7b8-9012-cdef-123456789012</example>
    [Required]
    public Guid ThresholdId { get; set; }
}
