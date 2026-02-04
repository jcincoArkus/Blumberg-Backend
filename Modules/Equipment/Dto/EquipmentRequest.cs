namespace Modules.Equipment.Dto;

/// <summary>
/// Equipment request DTO
/// </summary>
public class EquipmentRequest
{
    /// <summary>
    /// Equipment name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Equipment type
    /// </summary>
    public string EquipmentType { get; set; } = string.Empty;

    /// <summary>
    /// Site ID this equipment belongs to
    /// </summary>
    public Guid SiteId { get; set; }
}
