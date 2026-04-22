using System.ComponentModel.DataAnnotations;

namespace Modules.Admins.Dto;

/// <summary>
/// Request DTO for admin information
/// </summary>
public class AdminRequest
{
    /// <summary>
    /// Admin email address
    /// </summary>
    /// <example>admin@example.com</example>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Admin password
    /// </summary>
    /// <example>P@ssw0rd123!</example>
    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Admin first name
    /// </summary>
    /// <example>John</example>
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Admin last name
    /// </summary>
    /// <example>Doe</example>
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
}
