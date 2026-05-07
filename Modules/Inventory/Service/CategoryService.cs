using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Modules.Inventory.Repository;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Service;

public class CategoryService(ICategoryRepository repository, ILogger<CategoryService> logger) : ICategoryService
{
    [Span]
    public virtual async Task<(IReadOnlyList<InventoryCategory> Items, int TotalCount)> GetAllAsync(PaginationRequest request)
    {
        logger.LogDebug("Getting categories page {Page}", request.Page);
        return await repository.GetPagedAsync(request);
    }

    [Span]
    public virtual async Task<InventoryCategory> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
        {
            logger.LogWarning("Category not found: {Id}", id);
            throw new KeyNotFoundException($"Category with ID {id} was not found");
        }
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<InventoryCategory> CreateAsync(CategoryRequest request)
    {
        logger.LogDebug("Creating category: {Name}", request.Name);
        var entity = new InventoryCategory { Name = request.Name, Color = request.Color };
        return await repository.CreateAsync(entity);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<InventoryCategory> UpdateAsync(Guid id, CategoryRequest request)
    {
        var entity = await GetByIdAsync(id);
        entity.Name = request.Name;
        entity.Color = request.Color;
        return await repository.UpdateAsync(entity);
    }

    [Span]
    public virtual async Task DeleteAsync(Guid id)
    {
        var deleted = await repository.DeleteAsync(id);
        if (!deleted)
        {
            logger.LogWarning("Category not found for deletion: {Id}", id);
            throw new KeyNotFoundException($"Category with ID {id} was not found");
        }
    }
}
