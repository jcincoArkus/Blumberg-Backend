using Adapters.Jwt;
using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Auth.Dto;
using Modules.Auth.Repository;

namespace Modules.Auth.Service;

/// <summary>
/// Service implementation for authentication operations
/// </summary>
public class AuthService(IAdminRepository adminRepository, IJwtService jwtService, ILogger<AuthService> logger) : IAuthService
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        logger.LogDebug("Attempting to authenticate user with email: {Email}", request.Email);

        var admin = await adminRepository.GetByEmailAsync(request.Email);

        if (admin == null)
        {
            logger.LogWarning("Authentication failed: user not found with email: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
        {
            logger.LogWarning("Authentication failed: invalid password for email: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var token = jwtService.GenerateToken(
            admin.Id,
            admin.Email,
            admin.FirstName,
            admin.LastName,
            admin.OrganizationId);

        logger.LogInformation("User authenticated successfully: {Email}, AdminId: {AdminId}", admin.Email, admin.Id);

        return new AuthResponse
        {
            Token = token,
            Email = admin.Email,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            OrganizationId = admin.OrganizationId,
            ExpiresAt = DateTime.UtcNow.AddHours(jwtService.ExpirationHours)
        };
    }
}

