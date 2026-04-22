using Modules.Auth.Dto;

namespace Modules.Auth.Service;

/// <summary>
/// Service interface for authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates an admin user
    /// </summary>
    /// <param name="request">Login request with email and password</param>
    /// <returns>Authentication response with access and refresh tokens</returns>
    /// <exception cref="UnauthorizedAccessException">When credentials are invalid</exception>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Issues new access and refresh tokens using a valid refresh token
    /// </summary>
    /// <param name="request">Refresh request containing the refresh token</param>
    /// <returns>New authentication response with rotated tokens</returns>
    /// <exception cref="UnauthorizedAccessException">When refresh token is invalid or expired</exception>
    Task<AuthResponse> RefreshAsync(RefreshRequest request);
}

