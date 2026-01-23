using Adapters.Jwt;
using Modules.Auth.Dto;
using Modules.Auth.Repository;

namespace Modules.Auth.Service;

/// <summary>
/// Service implementation for authentication operations
/// </summary>
public class AuthService(IAdminRepository adminRepository, IJwtService jwtService) : IAuthService
{
    /// <inheritdoc />
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var admin = await adminRepository.GetByEmailAsync(request.Email) ??
            throw new UnauthorizedAccessException("Invalid email or password");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        var token = jwtService.GenerateToken(
            admin.Id,
            admin.Email,
            admin.FirstName,
            admin.LastName);

        return new AuthResponse
        {
            Token = token,
            Email = admin.Email,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            ExpiresAt = DateTime.UtcNow.AddHours(jwtService.ExpirationHours)
        };
    }
}

