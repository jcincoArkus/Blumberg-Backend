namespace Modules.Auth.Dto;

/// <summary>
/// Response DTO for successful authentication
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT access token (short-lived)
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// JWT refresh token (long-lived); use with POST /api/v1/auth/refresh to obtain a new access token
    /// </summary>
    public string? RefreshToken { get; set; }

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
    /// Organization (tenant) ID the admin belongs to; included in JWT as orgId claim
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Token expiration time
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}

