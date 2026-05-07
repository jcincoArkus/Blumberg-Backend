using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Modules.Inventory.Repository;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Service;

public class SiteZoneService(ISiteZoneRepository repository, ILogger<SiteZoneService> logger) : ISiteZoneService
{
    [Span]
    public virtual async Task<(IReadOnlyList<SiteZone> Items, int TotalCount)> GetAllAsync(PaginationRequest request, Guid? siteId)
    {
        logger.LogDebug("Getting site zones, siteId={SiteId}", siteId);
        return await repository.GetPagedAsync(request, siteId);
    }

    [Span]
    public virtual async Task<SiteZone> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
        {
            logger.LogWarning("Site zone not found: {Id}", id);
            throw new KeyNotFoundException($"Site zone with ID {id} was not found");
        }
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<SiteZone> CreateAsync(SiteZoneRequest request)
    {
        logger.LogDebug("Creating site zone: {Name} for site {SiteId}", request.Name, request.SiteId);
        var entity = new SiteZone { SiteId = request.SiteId, Name = request.Name };
        return await repository.CreateAsync(entity);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<SiteZone> UpdateAsync(Guid id, SiteZoneRequest request)
    {
        var entity = await GetByIdAsync(id);
        entity.SiteId = request.SiteId;
        entity.Name = request.Name;
        return await repository.UpdateAsync(entity);
    }

    [Span]
    public virtual async Task DeleteAsync(Guid id)
    {
        var deleted = await repository.DeleteAsync(id);
        if (!deleted)
        {
            logger.LogWarning("Site zone not found for deletion: {Id}", id);
            throw new KeyNotFoundException($"Site zone with ID {id} was not found");
        }
    }
}
