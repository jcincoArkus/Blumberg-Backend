using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Modules.Ingestion.Repository;
using Shared.Entity;

namespace Modules.Ingestion.Service;

/// <summary>
/// Service for creating and validating API keys
/// </summary>
public class ApiKeyService(IApiKeyRepository apiKeyRepository, ILogger<ApiKeyService> logger) : IApiKeyService
{
    /// <summary>Prefix for all API keys so clients can identify them</summary>
    public const string KeyPrefix = "blumberg_";

    /// <inheritdoc />
    public virtual async Task<(ApiKey Entity, string RawKey)> CreateKeyAsync(
        Guid organizationId,
        string name,
        CancellationToken cancellationToken = default)
    {
        var rawKey = GenerateRawKey();
        var keyHash = HashKey(rawKey);

        var entity = new ApiKey
        {
            Id = Guid.NewGuid(),
            KeyHash = keyHash,
            OrganizationId = organizationId,
            Name = name.Trim(),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };

        await apiKeyRepository.CreateAsync(entity, cancellationToken);
        logger.LogInformation("Created API key {Id} for organization {OrgId}", entity.Id, organizationId);

        return (entity, rawKey);
    }

    /// <inheritdoc />
    public virtual async Task<Guid?> ValidateKeyAsync(string rawKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawKey) || !rawKey.StartsWith(KeyPrefix, StringComparison.Ordinal))
            return null;

        var hash = HashKey(rawKey);
        var key = await apiKeyRepository.GetByKeyHashAsync(hash, cancellationToken);
        return key?.OrganizationId;
    }

    private static string GenerateRawKey()
    {
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
            rng.GetBytes(bytes);
        var segment = Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        return KeyPrefix + segment;
    }

    private static string HashKey(string rawKey)
    {
        var bytes = Encoding.UTF8.GetBytes(rawKey);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
