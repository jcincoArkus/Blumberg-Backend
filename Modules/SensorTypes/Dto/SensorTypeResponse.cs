using Shared.Enums;

namespace Modules.SensorTypes.Dto;

/// <summary>
/// Sensor type response DTO
/// </summary>
public class SensorTypeResponse
{
    /// <summary>Sensor type ID</summary>
    public Guid Id { get; set; }

    /// <summary>Kind/category (e.g. Temperature, Humidity)</summary>
    public SensorTypeKind Type { get; set; }

    /// <summary>Unit of measurement</summary>
    public Unit Unit { get; set; }

    /// <summary>Created at</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Updated at</summary>
    public DateTime? UpdatedAt { get; set; }
}
