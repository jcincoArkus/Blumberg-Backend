using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Ingestion.Dto;
using Modules.Ingestion.Repository;
using Modules.Ingestion.Service;
using Shared.Abstractions;
using Shared.Dto;

namespace Modules.Ingestion.Controller;

/// <summary>
/// API key management for ingestion (create, list, revoke). Requires JWT; API keys cannot manage other keys.
/// </summary>
[ApiController]
[Route("api/v1/ingestion/api-keys")]
[Tags("Ingestion")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class IngestionApiKeysController(
    IApiKeyService apiKeyService,
    IApiKeyRepository apiKeyRepository,
    ITenantContext tenantContext,
    ILogger<IngestionApiKeysController> logger) : ControllerBase
{
    /// <summary>
    /// Create an API key for the current organization. The raw key is returned only once.
    /// </summary>
    [HttpPost(Name = "CreateIngestionApiKeyV1")]
    [ProducesResponseType(typeof(CreateApiKeyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateApiKeyResponse>> Create(
        [FromBody] CreateApiKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        var orgId = tenantContext.CurrentOrganizationId;
        if (orgId == null)
        {
            logger.LogWarning("Create API key called without tenant context");
            return Unauthorized(new { message = "Organization context is required" });
        }

        var name = request?.Name?.Trim();
        if (string.IsNullOrEmpty(name))
            return BadRequest(new { message = "Name is required" });

        var (entity, rawKey) = await apiKeyService.CreateKeyAsync(orgId.Value, name, cancellationToken);

        return CreatedAtAction(
            nameof(List),
            null,
            new CreateApiKeyResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                OrganizationId = entity.OrganizationId,
                CreatedAt = entity.CreatedAt,
                Key = rawKey
            });
    }

    /// <summary>
    /// List API keys for the current organization (paginated). Key values are never returned.
    /// </summary>
    [HttpGet(Name = "ListIngestionApiKeysV1")]
    [ProducesResponseType(typeof(PagedResponse<ApiKeyListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<ApiKeyListResponse>>> List(
        [FromQuery] PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var orgId = tenantContext.CurrentOrganizationId;
        if (orgId == null)
        {
            logger.LogWarning("List API keys called without tenant context");
            return Unauthorized(new { message = "Organization context is required" });
        }

        var (items, totalCount) = await apiKeyRepository.GetPagedByOrganizationAsync(orgId.Value, request, cancellationToken);
        return Ok(new PagedResponse<ApiKeyListResponse>
        {
            Items = items.Select(k => new ApiKeyListResponse
            {
                Id = k.Id,
                Name = k.Name,
                CreatedAt = k.CreatedAt,
                RevokedAt = k.RevokedAt
            }).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    /// <summary>
    /// Revoke an API key. Only keys belonging to the current organization can be revoked.
    /// </summary>
    [HttpDelete("{id}", Name = "RevokeIngestionApiKeyV1")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken = default)
    {
        var orgId = tenantContext.CurrentOrganizationId;
        if (orgId == null)
        {
            logger.LogWarning("Revoke API key called without tenant context");
            return Unauthorized(new { message = "Organization context is required" });
        }

        var revoked = await apiKeyRepository.RevokeAsync(id, orgId.Value, cancellationToken);
        if (!revoked)
            return NotFound(new { message = "API key not found or already revoked" });

        return NoContent();
    }
}
