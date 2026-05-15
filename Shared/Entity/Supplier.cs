namespace Shared.Entity;

/// <summary>
/// Product supplier / vendor
/// </summary>
public class Supplier : BaseEntity
{
    /// <summary>Supplier company name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Geographic latitude of the supplier location</summary>
    public decimal? Latitude { get; set; }

    /// <summary>Geographic longitude of the supplier location</summary>
    public decimal? Longitude { get; set; }

    /// <summary>Lots provided by this supplier</summary>
    public ICollection<Lot> Lots { get; set; } = new List<Lot>();

    /// <summary>Intake shipments from this supplier</summary>
    public ICollection<IntakeShipment> IntakeShipments { get; set; } = new List<IntakeShipment>();
}
