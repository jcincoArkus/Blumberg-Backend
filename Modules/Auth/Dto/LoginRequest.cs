using System.ComponentModel.DataAnnotations;

namespace Modules.Auth.Dto;

/// <summary>
/// Request DTO for admin login
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Admin email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Admin password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}

