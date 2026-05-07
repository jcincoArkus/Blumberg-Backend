using System.ComponentModel.DataAnnotations;

namespace Modules.Inventory.Dto;

/// <summary>Create / update an inventory site</summary>
public class InventorySiteRequest
{
    /// <summary>Site display name</summary>
    /// <example>CDMX · Iztapalapa</example>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
}
