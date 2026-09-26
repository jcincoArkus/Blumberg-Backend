namespace Modules.Inventory.Dto;

/// <summary>Shipment line read model</summary>
public class ShipmentLineResponse
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public string PoReference { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string? LotCode { get; set; }
    public decimal Qty { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal CostPerUnit { get; set; }
    public DateTime CreatedAt { get; set; }
}
