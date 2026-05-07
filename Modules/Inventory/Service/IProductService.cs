using Modules.Inventory.Dto;
using Shared.Entity;

namespace Modules.Inventory.Service;

public interface IProductService
{
    Task<(IReadOnlyList<InventoryProduct> Items, int TotalCount)> GetAllAsync(GetProductsRequest request);
    Task<InventoryProduct> GetByIdAsync(Guid id);
    Task<InventoryProduct> CreateAsync(ProductRequest request);
    Task<InventoryProduct> UpdateAsync(Guid id, ProductRequest request);
    Task DeleteAsync(Guid id);
}
