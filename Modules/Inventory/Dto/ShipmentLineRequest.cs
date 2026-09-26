using System.ComponentModel.DataAnnotations;

namespace Modules.Inventory.Dto;

/// <summary>Create / update a shipment line</summary>
public class ShipmentLineRequest
{
    /// <summary>Parent shipment ID</summary>
    [Required]
    public Guid ShipmentId { get; set; }

    /// <summary>Product ID</summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>Lot code (assigned when shipment is confirmed)</summary>
    [StringLength(50)]
    public string? LotCode { get; set; }

    /// <summary>Quantity received</summary>
    [Required]
    [Range(0.001, double.MaxValue)]
    public decimal Qty { get; set; }

    /// <summary>Unit: "kg" | "unit" | "box"</summary>
    /// <example>kg</example>
    [Required]
    [RegularExpression("^(kg|unit|box)$", ErrorMessage = "Unit must be 'kg', 'unit', or 'box'")]
    public string Unit { get; set; } = string.Empty;

    /// <summary>Cost per unit for this line</summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal CostPerUnit { get; set; }
}
