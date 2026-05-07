using System.ComponentModel.DataAnnotations;

namespace Modules.Inventory.Dto;

/// <summary>Create a stock movement record</summary>
public class MovementRequest
{
    /// <summary>Movement type: "intake" | "output" | "waste" | "adjustment" | "transfer"</summary>
    /// <example>intake</example>
    [Required]
    [RegularExpression("^(intake|output|waste|adjustment|transfer)$",
        ErrorMessage = "Type must be 'intake', 'output', 'waste', 'adjustment', or 'transfer'")]
    public string Type { get; set; } = string.Empty;

    /// <summary>When the movement occurred (UTC)</summary>
    [Required]
    public DateTime OccurredAt { get; set; }

    /// <summary>Product ID</summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>Quantity delta (positive = in, negative = out)</summary>
    [Required]
    public decimal Qty { get; set; }

    /// <summary>Unit: "kg" | "unit"</summary>
    /// <example>kg</example>
    [Required]
    [RegularExpression("^(kg|unit)$", ErrorMessage = "Unit must be 'kg' or 'unit'")]
    public string Unit { get; set; } = string.Empty;

    /// <summary>Optional lot code</summary>
    [StringLength(50)]
    public string? LotCode { get; set; }

    /// <summary>Source site ID</summary>
    [Required]
    public Guid SiteId { get; set; }

    /// <summary>Destination site ID (required for transfers)</summary>
    public Guid? DestSiteId { get; set; }

    /// <summary>Name of the person who performed the movement</summary>
    [Required]
    [StringLength(200)]
    public string PerformedBy { get; set; } = string.Empty;

    /// <summary>Optional note</summary>
    public string? Note { get; set; }
}
