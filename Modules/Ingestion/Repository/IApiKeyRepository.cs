using Shared.Dto;
using Shared.Entity;

namespace Modules.Ingestion.Repository;

/// <summary>
/// Repository for API keys (ingestion / machine-to-machine auth)
/// </summary>
public interface IApiKeyRepository
{
    /// <summary>Creates an API key; caller must set KeyHash, OrganizationId, Name, CreatedAt</summary>
    Task<ApiKey> CreateAsync(ApiKey apiKey, CancellationToken cancellationToken = default);

    /// <summary>Gets an active key by hash (no tenant filter; used during auth)</summary>
    Task<ApiKey?> GetByKeyHashAsync(string keyHash, CancellationToken cancellationToken = default);

    /// <summary>Lists keys for an organization (paginated)</summary>
    Task<(IReadOnlyList<ApiKey> Items, int TotalCount)> GetPagedByOrganizationAsync(
        Guid organizationId,
        PaginationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Sets RevokedAt to now for the given key if it belongs to the organization</summary>
    Task<bool> RevokeAsync(Guid id, Guid organizationId, CancellationToken cancellationToken = default);
}
