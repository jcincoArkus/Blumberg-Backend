namespace Modules.Admins.Dto;

/// <summary>
/// Request DTO for admin information
/// </summary>
public class AdminRequest
{
    /// <summary>
    /// Admin email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Admin password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Admin first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Admin last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
}
