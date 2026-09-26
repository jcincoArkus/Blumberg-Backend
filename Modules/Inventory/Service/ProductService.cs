using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Modules.Inventory.Repository;
using Shared.Entity;

namespace Modules.Inventory.Service;

public class ProductService(IProductRepository repository, ILogger<ProductService> logger) : IProductService
{
    [Span]
    public virtual async Task<(IReadOnlyList<InventoryProduct> Items, int TotalCount)> GetAllAsync(GetProductsRequest request)
    {
        logger.LogDebug("Getting products page {Page}", request.Page);
        return await repository.GetPagedAsync(request);
    }

    [Span]
    public virtual async Task<InventoryProduct> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
        {
            logger.LogWarning("Product not found: {Id}", id);
            throw new KeyNotFoundException($"Product with ID {id} was not found");
        }
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<InventoryProduct> CreateAsync(ProductRequest request)
    {
        if (await repository.ExistsBySkuAsync(request.Sku))
            throw new InvalidOperationException($"A product with SKU '{request.Sku}' already exists");

        var entity = new InventoryProduct
        {
            Sku = request.Sku,
            Name = request.Name,
            CategoryId = request.CategoryId,
            Unit = request.Unit,
            KgPerBox = request.KgPerBox,
            ShelfLifeDays = request.ShelfLifeDays,
            Price = request.Price
        };
        return await repository.CreateAsync(entity);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<InventoryProduct> UpdateAsync(Guid id, ProductRequest request)
    {
        var entity = await GetByIdAsync(id);

        if (entity.Sku != request.Sku && await repository.ExistsBySkuAsync(request.Sku, id))
            throw new InvalidOperationException($"A product with SKU '{request.Sku}' already exists");

        entity.Sku = request.Sku;
        entity.Name = request.Name;
        entity.CategoryId = request.CategoryId;
        entity.Unit = request.Unit;
        entity.KgPerBox = request.KgPerBox;
        entity.ShelfLifeDays = request.ShelfLifeDays;
        entity.Price = request.Price;
        return await repository.UpdateAsync(entity);
    }

    [Span]
    public virtual async Task DeleteAsync(Guid id)
    {
        var deleted = await repository.DeleteAsync(id);
        if (!deleted)
        {
            logger.LogWarning("Product not found for deletion: {Id}", id);
            throw new KeyNotFoundException($"Product with ID {id} was not found");
        }
    }
}
