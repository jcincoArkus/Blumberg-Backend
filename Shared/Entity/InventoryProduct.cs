namespace Shared.Entity;

/// <summary>
/// SKU / product catalogue entry
/// </summary>
public class InventoryProduct : BaseEntity
{
    /// <summary>Stock-keeping unit code (unique)</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Product display name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Category this product belongs to</summary>
    public Guid CategoryId { get; set; }

    /// <summary>Navigation to category</summary>
    public InventoryCategory Category { get; set; } = null!;

    /// <summary>Base unit of measure: "kg" | "unit" | "box"</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>Kg per box when Unit = "box"; null otherwise</summary>
    public decimal? KgPerBox { get; set; }

    /// <summary>Expected shelf life in days from entry date</summary>
    public int ShelfLifeDays { get; set; }

    /// <summary>Sale price</summary>
    public decimal Price { get; set; }

    /// <summary>Lots of this product</summary>
    public ICollection<Lot> Lots { get; set; } = new List<Lot>();

    /// <summary>Stock movements for this product</summary>
    public ICollection<Movement> Movements { get; set; } = new List<Movement>();

    /// <summary>Shipment lines that reference this product</summary>
    public ICollection<IntakeShipmentLine> ShipmentLines { get; set; } = new List<IntakeShipmentLine>();
}
