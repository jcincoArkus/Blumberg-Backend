using System.ComponentModel.DataAnnotations;

namespace Modules.Inventory.Dto;

/// <summary>Create / update a product (SKU catalogue entry)</summary>
public class ProductRequest
{
    /// <summary>SKU code (unique)</summary>
    /// <example>AVO-HASS</example>
    [Required]
    [StringLength(50)]
    public string Sku { get; set; } = string.Empty;

    /// <summary>Product display name</summary>
    /// <example>Avocado · Hass</example>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Category ID</summary>
    [Required]
    public Guid CategoryId { get; set; }

    /// <summary>Unit of measure: "kg" | "unit" | "box"</summary>
    /// <example>kg</example>
    [Required]
    [RegularExpression("^(kg|unit|box)$", ErrorMessage = "Unit must be 'kg', 'unit', or 'box'")]
    public string Unit { get; set; } = string.Empty;

    /// <summary>Kg per box — required when Unit is "box"</summary>
    public decimal? KgPerBox { get; set; }

    /// <summary>Expected shelf life in days</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int ShelfLifeDays { get; set; }

    /// <summary>Sale price</summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}
