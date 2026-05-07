namespace Shared.Entity;

/// <summary>
/// Inventory site / warehouse (distinct from the monitoring Site entity)
/// </summary>
public class InventorySite : BaseEntity
{
    /// <summary>Human-readable name, e.g. "CDMX · Iztapalapa"</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Zones within this site</summary>
    public ICollection<SiteZone> Zones { get; set; } = new List<SiteZone>();

    /// <summary>Lots stored at this site</summary>
    public ICollection<Lot> Lots { get; set; } = new List<Lot>();

    /// <summary>Intake shipments received at this site</summary>
    public ICollection<IntakeShipment> IntakeShipments { get; set; } = new List<IntakeShipment>();

    /// <summary>Movements originating from this site</summary>
    public ICollection<Movement> Movements { get; set; } = new List<Movement>();
}
