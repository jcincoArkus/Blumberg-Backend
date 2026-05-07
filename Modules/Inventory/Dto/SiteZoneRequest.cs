using System.ComponentModel.DataAnnotations;

namespace Modules.Inventory.Dto;

/// <summary>Create / update a site zone</summary>
public class SiteZoneRequest
{
    /// <summary>Parent inventory site ID</summary>
    [Required]
    public Guid SiteId { get; set; }

    /// <summary>Zone name (unique within the site)</summary>
    /// <example>Cold Room A</example>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}
