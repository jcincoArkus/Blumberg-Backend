namespace Shared.Entity;

/// <summary>
/// Append-only audit trail of every stock change.
/// Positive qty = stock in; negative qty = stock out.
/// </summary>
public class Movement : BaseEntity
{
    /// <summary>Movement type: "intake" | "output" | "waste" | "adjustment" | "transfer"</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>When the movement occurred</summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>Product affected</summary>
    public Guid ProductId { get; set; }

    /// <summary>Navigation to product</summary>
    public InventoryProduct Product { get; set; } = null!;

    /// <summary>Quantity delta (positive = in, negative = out)</summary>
    public decimal Qty { get; set; }

    /// <summary>Unit of measure: "kg" | "unit"</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>Optional lot this movement is linked to</summary>
    public string? LotCode { get; set; }

    /// <summary>Navigation to lot</summary>
    public Lot? Lot { get; set; }

    /// <summary>Source site</summary>
    public Guid SiteId { get; set; }

    /// <summary>Navigation to source site</summary>
    public InventorySite Site { get; set; } = null!;

    /// <summary>Destination site (transfers only)</summary>
    public Guid? DestSiteId { get; set; }

    /// <summary>Navigation to destination site</summary>
    public InventorySite? DestSite { get; set; }

    /// <summary>Who performed the movement</summary>
    public string PerformedBy { get; set; } = string.Empty;

    /// <summary>Optional free-text note</summary>
    public string? Note { get; set; }
}
