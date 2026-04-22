using System.ComponentModel.DataAnnotations;
using Shared.Enums;

namespace Modules.SensorTypes.Dto;

/// <summary>
/// Sensor type request DTO
/// </summary>
public class SensorTypeRequest
{
    /// <summary>
    /// Kind/category of sensor (e.g. Temperature, Humidity)
    /// </summary>
    /// <example>Temperature</example>
    [Required]
    public SensorTypeKind Type { get; set; }

    /// <summary>
    /// Unit of measurement for this sensor type
    /// </summary>
    /// <example>Celsius</example>
    [Required]
    public Unit Unit { get; set; }
}
