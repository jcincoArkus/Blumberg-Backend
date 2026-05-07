namespace Shared.Entity;

/// <summary>
/// Header record for a receiving event (a truck load / PO delivery)
/// </summary>
public class IntakeShipment : BaseEntity
{
    /// <summary>Purchase-order reference (unique)</summary>
    public string PoReference { get; set; } = string.Empty;

    /// <summary>Supplier who sent the shipment</summary>
    public Guid SupplierId { get; set; }

    /// <summary>Navigation to supplier</summary>
    public Supplier Supplier { get; set; } = null!;

    /// <summary>Vehicle plate / description</summary>
    public string? Vehicle { get; set; }

    /// <summary>Driver name</summary>
    public string? Driver { get; set; }

    /// <summary>Receiving site</summary>
    public Guid SiteId { get; set; }

    /// <summary>Navigation to site</summary>
    public InventorySite Site { get; set; } = null!;

    /// <summary>Zone where goods were unloaded</summary>
    public string ReceivingZone { get; set; } = string.Empty;

    /// <summary>Cold-chain temperature recorded on arrival (°C)</summary>
    public decimal? ColdChainTempC { get; set; }

    /// <summary>Timestamp the vehicle arrived</summary>
    public DateTime ArrivedAt { get; set; }

    /// <summary>Staff member who received the shipment</summary>
    public string ReceivedBy { get; set; } = string.Empty;

    /// <summary>Status: "draft" | "received" | "cancelled"</summary>
    public string Status { get; set; } = "draft";

    /// <summary>Line items in this shipment</summary>
    public ICollection<IntakeShipmentLine> Lines { get; set; } = new List<IntakeShipmentLine>();
}
