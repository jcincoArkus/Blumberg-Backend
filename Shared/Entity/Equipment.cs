namespace Shared.Entity;

/// <summary>
/// Equipment entity representing a piece of equipment
/// </summary>
public class Equipment : BaseEntity
{
    /// <summary>
    /// Equipment's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Equipment's serial number
    /// </summary>
    public string EquipmentType { get; set; } = string.Empty;

    /// <summary>
    /// Organization this equipment belongs to
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Navigation to the organization
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// Site this equipment belongs to
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// Navigation to the site
    /// </summary>
    public Site Site { get; set; } = null!;

    /// <summary>
    /// Sensors contained in this equipment
    /// </summary>
    public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
}