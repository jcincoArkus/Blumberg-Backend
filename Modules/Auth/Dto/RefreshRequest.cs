namespace Modules.Auth.Dto;

/// <summary>
/// Request body for refreshing an access token using a refresh token
/// </summary>
public class RefreshRequest
{
    /// <summary>
    /// The refresh token previously returned from login or refresh
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
