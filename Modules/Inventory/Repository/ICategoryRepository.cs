using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public interface ICategoryRepository
{
    Task<(IReadOnlyList<InventoryCategory> Items, int TotalCount)> GetPagedAsync(PaginationRequest request);
    Task<InventoryCategory?> GetByIdAsync(Guid id);
    Task<InventoryCategory> CreateAsync(InventoryCategory entity);
    Task<InventoryCategory> UpdateAsync(InventoryCategory entity);
    Task<bool> DeleteAsync(Guid id);
}
