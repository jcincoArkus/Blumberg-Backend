using System.ComponentModel.DataAnnotations;

namespace Modules.Sites.Dto;

/// <summary>
/// Site request DTO
/// </summary>
public class SiteRequest
{
    /// <summary>
    /// Site name
    /// </summary>
    /// <example>Main Office</example>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Site address
    /// </summary>
    /// <example>123 Main St</example>
    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Site city
    /// </summary>
    /// <example>Austin</example>
    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Site state
    /// </summary>
    /// <example>TX</example>
    [Required]
    [StringLength(100)]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Site postal code
    /// </summary>
    /// <example>78701</example>
    [Required]
    [StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Site country
    /// </summary>
    /// <example>US</example>
    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;
}
