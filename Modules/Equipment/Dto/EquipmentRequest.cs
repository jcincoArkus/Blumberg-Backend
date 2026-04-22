using System.ComponentModel.DataAnnotations;

namespace Modules.Equipment.Dto;

/// <summary>
/// Equipment request DTO
/// </summary>
public class EquipmentRequest
{
    /// <summary>
    /// Equipment name
    /// </summary>
    /// <example>Boiler Unit A</example>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Equipment type
    /// </summary>
    /// <example>Boiler</example>
    [Required]
    [StringLength(100)]
    public string EquipmentType { get; set; } = string.Empty;

    /// <summary>
    /// Site ID this equipment belongs to
    /// </summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    [Required]
    public Guid SiteId { get; set; }
}
