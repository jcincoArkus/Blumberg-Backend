namespace Shared.Entity;

/// <summary>
/// One product line within an intake shipment
/// </summary>
public class IntakeShipmentLine : BaseEntity
{
    /// <summary>Parent shipment</summary>
    public Guid ShipmentId { get; set; }

    /// <summary>Navigation to parent shipment</summary>
    public IntakeShipment Shipment { get; set; } = null!;

    /// <summary>Product received</summary>
    public Guid ProductId { get; set; }

    /// <summary>Navigation to product</summary>
    public InventoryProduct Product { get; set; } = null!;

    /// <summary>Lot assigned when shipment is confirmed (null while draft)</summary>
    public string? LotCode { get; set; }

    /// <summary>Navigation to lot</summary>
    public Lot? Lot { get; set; }

    /// <summary>Quantity received</summary>
    public decimal Qty { get; set; }

    /// <summary>Unit: "kg" | "unit" | "box"</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>Cost per unit for this line</summary>
    public decimal CostPerUnit { get; set; }
}
