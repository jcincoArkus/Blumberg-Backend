using Shared.Entity;

namespace Modules.Sites.Dto;

/// <summary>
/// Site request DTO
/// </summary>
public class SiteRequest
{
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
}