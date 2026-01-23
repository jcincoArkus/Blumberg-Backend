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
    /// <returns>Authentication response with token</returns>
    /// <exception cref="UnauthorizedAccessException">When credentials are invalid</exception>
    Task<AuthResponse> LoginAsync(LoginRequest request);
}

