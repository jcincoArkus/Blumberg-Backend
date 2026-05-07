using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Modules.Inventory.Repository;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Service;

public class InventorySiteService(IInventorySiteRepository repository, ILogger<InventorySiteService> logger) : IInventorySiteService
{
    [Span]
    public virtual async Task<(IReadOnlyList<InventorySite> Items, int TotalCount)> GetAllAsync(PaginationRequest request)
    {
        logger.LogDebug("Getting inventory sites page {Page}", request.Page);
        return await repository.GetPagedAsync(request);
    }

    [Span]
    public virtual async Task<InventorySite> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
        {
            logger.LogWarning("Inventory site not found: {Id}", id);
            throw new KeyNotFoundException($"Inventory site with ID {id} was not found");
        }
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<InventorySite> CreateAsync(InventorySiteRequest request)
    {
        logger.LogDebug("Creating inventory site: {Name}", request.Name);
        var entity = new InventorySite { Name = request.Name };
        return await repository.CreateAsync(entity);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<InventorySite> UpdateAsync(Guid id, InventorySiteRequest request)
    {
        var entity = await GetByIdAsync(id);
        entity.Name = request.Name;
        return await repository.UpdateAsync(entity);
    }

    [Span]
    public virtual async Task DeleteAsync(Guid id)
    {
        var deleted = await repository.DeleteAsync(id);
        if (!deleted)
        {
            logger.LogWarning("Inventory site not found for deletion: {Id}", id);
            throw new KeyNotFoundException($"Inventory site with ID {id} was not found");
        }
    }
}
