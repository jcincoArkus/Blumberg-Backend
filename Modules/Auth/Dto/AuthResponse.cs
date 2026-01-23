namespace Modules.Auth.Dto;

/// <summary>
/// Response DTO for successful authentication
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Admin email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Admin first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Admin last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}

