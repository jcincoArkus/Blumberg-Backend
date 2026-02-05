namespace Adapters.Config;

/// <summary>
/// JWT (JSON Web Token) authentication configuration
/// </summary>
/// <remarks>
/// Configures JWT token generation and validation parameters.
/// All values can be set via environment variables with JWT_ prefix.
/// </remarks>
public class JwtConfig
{
    /// <summary>
    /// Secret key for signing JWT tokens (required, min 32 characters)
    /// </summary>
    /// <remarks>
    /// IMPORTANT: Use a strong, random secret key in production.
    /// Never commit this value to source control.
    /// Generate with: openssl rand -base64 32
    /// </remarks>
    /// <example>your-super-secret-jwt-key-change-this-in-production-min-32-chars</example>
    public string SecretKey { get; private set; } = string.Empty;

    /// <summary>
    /// JWT token issuer (who created the token)
    /// </summary>
    /// <remarks>
    /// Typically your API domain or application name.
    /// Used for token validation.
    /// </remarks>
    /// <example>Blumberg.API, api.blumberg.com</example>
    public string Issuer { get; private set; } = "Blumberg.API";

    /// <summary>
    /// JWT token audience (who the token is intended for)
    /// </summary>
    /// <remarks>
    /// Typically your client application name or domain.
    /// Used for token validation.
    /// </remarks>
    /// <example>Blumberg.Client, app.blumberg.com</example>
    public string Audience { get; private set; } = "Blumberg.Client";

    /// <summary>
    /// Token expiration time in hours
    /// </summary>
    /// <remarks>
    /// Default: 24 hours
    /// Recommended: 1-24 hours for web apps, shorter for mobile apps
    /// </remarks>
    /// <example>24, 12, 1</example>
    public int ExpirationHours { get; private set; } = 24;

    /// <summary>
    /// Initializes configuration from environment variables
    /// </summary>
    public JwtConfig Init()
    {
        SecretKey = EnvHelper.GetEnvRequired("JWT_SECRET_KEY");
        Issuer = EnvHelper.GetEnv("JWT_ISSUER", "Blumberg.API");
        Audience = EnvHelper.GetEnv("JWT_AUDIENCE", "Blumberg.Client");
        ExpirationHours = EnvHelper.GetEnvInt("JWT_EXPIRATION_HOURS", 24);

        return this;
    }

    /// <summary>
    /// Validates JWT configuration
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid</exception>
    public JwtConfig Validate()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
            throw new InvalidOperationException("JWT secret key (JWT_SECRET_KEY) is required");

        if (SecretKey.Length < 32)
            throw new InvalidOperationException(
                $"JWT secret key must be at least 32 characters long (current: {SecretKey.Length})");

        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("JWT issuer (JWT_ISSUER) is required");

        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("JWT audience (JWT_AUDIENCE) is required");

        switch (ExpirationHours)
        {
            case <= 0:
                throw new InvalidOperationException("JWT expiration hours must be greater than 0");
            // 30 days
            case > 720:
                Console.WriteLine("WARNING: JWT expiration is set to more than 30 days. Consider using refresh tokens.");
                break;
        }

        return this;
    }
}

