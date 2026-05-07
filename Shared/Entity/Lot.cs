namespace Shared.Entity;

/// <summary>
/// A batch of a product received on a specific date.
/// Uses a natural string primary key following pattern L-YYMMDD-NN.
/// </summary>
public class Lot
{
    /// <summary>Natural PK — lot code (e.g. L-250501-01)</summary>
    public string LotCode { get; set; } = string.Empty;

    /// <summary>Product in this lot</summary>
    public Guid ProductId { get; set; }

    /// <summary>Navigation to product</summary>
    public InventoryProduct Product { get; set; } = null!;

    /// <summary>Quantity on hand</summary>
    public decimal Qty { get; set; }

    /// <summary>Unit of measure: "kg" | "unit"</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>Date/time the lot entered the warehouse</summary>
    public DateTime EntryAt { get; set; }

    /// <summary>Expiry date/time</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Site where the lot is stored</summary>
    public Guid SiteId { get; set; }

    /// <summary>Navigation to site</summary>
    public InventorySite Site { get; set; } = null!;

    /// <summary>Storage zone name within the site</summary>
    public string Zone { get; set; } = string.Empty;

    /// <summary>Optional supplier</summary>
    public Guid? SupplierId { get; set; }

    /// <summary>Navigation to supplier</summary>
    public Supplier? Supplier { get; set; }

    /// <summary>Cost per unit when this lot was received</summary>
    public decimal CostPerUnit { get; set; }

    /// <summary>Record creation timestamp</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Record last-updated timestamp</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Movements referencing this lot</summary>
    public ICollection<Movement> Movements { get; set; } = new List<Movement>();

    /// <summary>Shipment lines that generated this lot</summary>
    public ICollection<IntakeShipmentLine> ShipmentLines { get; set; } = new List<IntakeShipmentLine>();
}
