namespace Adapters.Jwt;

/// <summary>
/// Interface for JWT token generation and validation
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Token expiration in hours
    /// </summary>
    int ExpirationHours { get; }

    /// <summary>
    /// Generates a JWT token for the given user
    /// </summary>
    /// <param name="userId">User unique identifier</param>
    /// <param name="email">User email</param>
    /// <param name="firstName">User first name</param>
    /// <param name="lastName">User last name</param>
    /// <param name="organizationId">Organization (tenant) ID for scoping</param>
    /// <returns>JWT token string</returns>
    string GenerateToken(Guid userId, string email, string firstName, string lastName, Guid organizationId);

    /// <summary>
    /// Validates a JWT token
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool ValidateToken(string token);

    /// <summary>
    /// Extracts user ID from a JWT token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>User ID if valid, null otherwise</returns>
    Guid? GetUserIdFromToken(string token);

    /// <summary>
    /// Generates a long-lived refresh token for the given user (same claims as access token plus token_type=refresh).
    /// </summary>
    string GenerateRefreshToken(Guid userId, string email, string firstName, string lastName, Guid organizationId);

    /// <summary>
    /// Validates that the token is a valid refresh token (signature, lifetime, and token_type=refresh).
    /// </summary>
    /// <param name="token">JWT refresh token</param>
    /// <returns>True if valid refresh token, false otherwise</returns>
    bool ValidateRefreshToken(string token);
}

