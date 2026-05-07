namespace Modules.Inventory.Dto;

/// <summary>Inventory site read model</summary>
public class InventorySiteResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
