namespace Modules.Ingestion.Service;

/// <summary>
/// Service for creating and validating API keys (ingestion / M2M auth)
/// </summary>
public interface IApiKeyService
{
    /// <summary>
    /// Creates a new API key for the organization. Returns the raw key only once; it cannot be retrieved later.
    /// </summary>
    /// <param name="organizationId">Organization the key belongs to</param>
    /// <param name="name">Human-readable name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of (key entity without raw key, raw key string to show once)</returns>
    Task<(Shared.Entity.ApiKey Entity, string RawKey)> CreateKeyAsync(
        Guid organizationId,
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a raw key and returns the organization ID if valid and not revoked.
    /// </summary>
    /// <param name="rawKey">The raw API key (e.g. blumberg_xxx)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Organization ID if valid, null otherwise</returns>
    Task<Guid?> ValidateKeyAsync(string rawKey, CancellationToken cancellationToken = default);
}
