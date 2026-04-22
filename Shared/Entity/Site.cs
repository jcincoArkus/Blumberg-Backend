namespace Shared.Entity;

/// <summary>
/// Site entity representing a site
/// </summary>
public class Site : BaseEntity
{
    /// <summary>
    /// Site's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Site's Address
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Site's City
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Site's State
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Site's Postal Code
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Site's Country
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Organization this site belongs to
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Navigation to the organization
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// Equipment at this site
    /// </summary>
    public ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
}