namespace Modules.Sites.Dto;

/// <summary>
/// Site response DTO (no nested entities to avoid JSON cycles)
/// </summary>
public class SiteResponse
{
    /// <summary>
    /// Site ID
    /// </summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Site name
    /// </summary>
    /// <example>Main Office</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Site address
    /// </summary>
    /// <example>123 Main St</example>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Site city
    /// </summary>
    /// <example>Austin</example>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Site state
    /// </summary>
    /// <example>TX</example>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Site postal code
    /// </summary>
    /// <example>78701</example>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Site country
    /// </summary>
    /// <example>US</example>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Site organization ID
    /// </summary>
    /// <example>b2c3d4e5-f6a7-8901-bcde-f12345678901</example>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Organization name (for display)
    /// </summary>
    /// <example>Acme Corp</example>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Site created at
    /// </summary>
    /// <example>2025-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Site updated at
    /// </summary>
    /// <example>2025-01-16T14:00:00Z</example>
    public DateTime? UpdatedAt { get; set; }
}
