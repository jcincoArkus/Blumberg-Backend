using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Modules.Inventory.Repository;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Service;

public class SupplierService(ISupplierRepository repository, ILogger<SupplierService> logger) : ISupplierService
{
    [Span]
    public virtual async Task<(IReadOnlyList<Supplier> Items, int TotalCount)> GetAllAsync(PaginationRequest request)
    {
        logger.LogDebug("Getting suppliers page {Page}", request.Page);
        return await repository.GetPagedAsync(request);
    }

    [Span]
    public virtual async Task<Supplier> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
        {
            logger.LogWarning("Supplier not found: {Id}", id);
            throw new KeyNotFoundException($"Supplier with ID {id} was not found");
        }
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<Supplier> CreateAsync(SupplierRequest request)
    {
        logger.LogDebug("Creating supplier: {Name}", request.Name);
        var entity = new Supplier { Name = request.Name };
        return await repository.CreateAsync(entity);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<Supplier> UpdateAsync(Guid id, SupplierRequest request)
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
            logger.LogWarning("Supplier not found for deletion: {Id}", id);
            throw new KeyNotFoundException($"Supplier with ID {id} was not found");
        }
    }
}
