using System.ComponentModel.DataAnnotations;

namespace Modules.Inventory.Dto;

/// <summary>Create / update an inventory category</summary>
public class CategoryRequest
{
    /// <summary>Category name</summary>
    /// <example>Fruit</example>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>UI colour token (e.g. "teal", "blue-gray", "yellow")</summary>
    /// <example>teal</example>
    [Required]
    [StringLength(50)]
    public string Color { get; set; } = string.Empty;
}
