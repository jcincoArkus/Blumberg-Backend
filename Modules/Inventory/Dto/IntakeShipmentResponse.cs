namespace Modules.Inventory.Dto;

/// <summary>Intake shipment header read model</summary>
public class IntakeShipmentResponse
{
    public Guid Id { get; set; }
    public string PoReference { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? Vehicle { get; set; }
    public string? Driver { get; set; }
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string ReceivingZone { get; set; } = string.Empty;
    public decimal? ColdChainTempC { get; set; }
    public DateTime ArrivedAt { get; set; }
    public string ReceivedBy { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
