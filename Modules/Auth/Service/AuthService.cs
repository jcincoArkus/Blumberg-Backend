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

        var accessToken = jwtService.GenerateToken(
            admin.Id,
            admin.Email,
            admin.FirstName,
            admin.LastName,
            admin.OrganizationId);

        var refreshToken = jwtService.GenerateRefreshToken(
            admin.Id,
            admin.Email,
            admin.FirstName,
            admin.LastName,
            admin.OrganizationId);

        logger.LogInformation("User authenticated successfully: {Email}, AdminId: {AdminId}", admin.Email, admin.Id);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Email = admin.Email,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            OrganizationId = admin.OrganizationId,
            ExpiresAt = DateTime.UtcNow.AddHours(jwtService.ExpirationHours)
        };
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<AuthResponse> RefreshAsync(RefreshRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            logger.LogWarning("Refresh attempted with empty token");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        if (!jwtService.ValidateRefreshToken(request.RefreshToken))
        {
            logger.LogWarning("Refresh attempted with invalid or expired refresh token");
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        var userId = jwtService.GetUserIdFromToken(request.RefreshToken);
        if (userId == null)
        {
            logger.LogWarning("Refresh token valid but could not extract user id");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Use GetByIdForAuthAsync: refresh request has no JWT so tenant context is null;
        // the Admin query filter would exclude all rows (WHERE FALSE) and we'd always get 401.
        var admin = await adminRepository.GetByIdForAuthAsync(userId.Value);
        if (admin == null)
        {
            logger.LogWarning("Refresh attempted for non-existent admin: {UserId}", userId);
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var accessToken = jwtService.GenerateToken(
            admin.Id,
            admin.Email,
            admin.FirstName,
            admin.LastName,
            admin.OrganizationId);

        var refreshToken = jwtService.GenerateRefreshToken(
            admin.Id,
            admin.Email,
            admin.FirstName,
            admin.LastName,
            admin.OrganizationId);

        logger.LogDebug("Tokens refreshed for admin: {AdminId}", admin.Id);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Email = admin.Email,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            OrganizationId = admin.OrganizationId,
            ExpiresAt = DateTime.UtcNow.AddHours(jwtService.ExpirationHours)
        };
    }
}

