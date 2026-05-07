namespace Shared.Entity;

/// <summary>
/// Inventory category (e.g. Fruit, Vegetables, Citrus)
/// </summary>
public class InventoryCategory : BaseEntity
{
    /// <summary>Display name of the category</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>UI colour token (e.g. "teal", "blue-gray")</summary>
    public string Color { get; set; } = string.Empty;

    /// <summary>Products belonging to this category</summary>
    public ICollection<InventoryProduct> Products { get; set; } = new List<InventoryProduct>();
}
