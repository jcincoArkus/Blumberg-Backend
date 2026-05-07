namespace Shared.Entity;

/// <summary>
/// Named area within an inventory site (e.g. "Cold Room A", "Dock 3")
/// </summary>
public class SiteZone : BaseEntity
{
    /// <summary>Parent site</summary>
    public Guid SiteId { get; set; }

    /// <summary>Navigation to parent site</summary>
    public InventorySite Site { get; set; } = null!;

    /// <summary>Zone name (unique per site)</summary>
    public string Name { get; set; } = string.Empty;
}
