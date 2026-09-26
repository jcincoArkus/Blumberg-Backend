using Modules.Inventory.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public interface IProductRepository
{
    Task<(IReadOnlyList<InventoryProduct> Items, int TotalCount)> GetPagedAsync(GetProductsRequest request);
    Task<InventoryProduct?> GetByIdAsync(Guid id);
    Task<InventoryProduct> CreateAsync(InventoryProduct entity);
    Task<InventoryProduct> UpdateAsync(InventoryProduct entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsBySkuAsync(string sku, Guid? excludeId = null);
}
