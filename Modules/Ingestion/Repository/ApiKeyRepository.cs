using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Ingestion.Repository;

/// <summary>
/// Repository implementation for API keys
/// </summary>
public class ApiKeyRepository(ApplicationDbContext context, ILogger<ApiKeyRepository> logger) : IApiKeyRepository
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<ApiKey> CreateAsync(ApiKey apiKey, CancellationToken cancellationToken = default)
    {
        if (apiKey.Id == Guid.Empty)
            apiKey.Id = Guid.NewGuid();

        context.ApiKeys.Add(apiKey);
        await context.SaveChangesAsync(cancellationToken);
        return apiKey;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<ApiKey?> GetByKeyHashAsync(string keyHash, CancellationToken cancellationToken = default)
    {
        return await context.ApiKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.KeyHash == keyHash && e.RevokedAt == null,
                cancellationToken);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<ApiKey> Items, int TotalCount)> GetPagedByOrganizationAsync(
        Guid organizationId,
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = context.ApiKeys
            .AsNoTracking()
            .Where(e => e.OrganizationId == organizationId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<bool> RevokeAsync(Guid id, Guid organizationId, CancellationToken cancellationToken = default)
    {
        var key = await context.ApiKeys
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganizationId == organizationId, cancellationToken);
        if (key == null)
        {
            logger.LogWarning("API key {Id} not found or wrong org for revoke", id);
            return false;
        }

        key.RevokedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Revoked API key {Id}", id);
        return true;
    }
}
