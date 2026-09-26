using System.ComponentModel.DataAnnotations;

namespace Modules.Inventory.Dto;

/// <summary>Create / update a lot</summary>
public class LotRequest
{
    /// <summary>Lot code (natural key, e.g. L-250501-01)</summary>
    /// <example>L-250501-01</example>
    [Required]
    [StringLength(50)]
    public string LotCode { get; set; } = string.Empty;

    /// <summary>Product ID</summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>Quantity</summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Qty { get; set; }

    /// <summary>Unit: "kg" | "unit"</summary>
    /// <example>kg</example>
    [Required]
    [RegularExpression("^(kg|unit)$", ErrorMessage = "Unit must be 'kg' or 'unit'")]
    public string Unit { get; set; } = string.Empty;

    /// <summary>Entry date/time (UTC)</summary>
    [Required]
    public DateTime EntryAt { get; set; }

    /// <summary>Expiry date/time (UTC)</summary>
    [Required]
    public DateTime ExpiresAt { get; set; }

    /// <summary>Inventory site ID</summary>
    [Required]
    public Guid SiteId { get; set; }

    /// <summary>Zone name within the site</summary>
    [Required]
    [StringLength(100)]
    public string Zone { get; set; } = string.Empty;

    /// <summary>Optional supplier ID</summary>
    public Guid? SupplierId { get; set; }

    /// <summary>Cost per unit at receipt</summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal CostPerUnit { get; set; }
}
