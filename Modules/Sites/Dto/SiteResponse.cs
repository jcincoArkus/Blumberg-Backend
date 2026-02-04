namespace Modules.Sites.Dto;

/// <summary>
/// Site response DTO (no nested entities to avoid JSON cycles)
/// </summary>
public class SiteResponse
{
    /// <summary>
    /// Site ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Site name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Site address
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Site city
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Site state
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Site postal code
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Site country
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Site organization ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Organization name (for display)
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Site created at
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Site updated at
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}