using Modules.Inventory.Dto;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Service;

public interface ICategoryService
{
    Task<(IReadOnlyList<InventoryCategory> Items, int TotalCount)> GetAllAsync(PaginationRequest request);
    Task<InventoryCategory> GetByIdAsync(Guid id);
    Task<InventoryCategory> CreateAsync(CategoryRequest request);
    Task<InventoryCategory> UpdateAsync(Guid id, CategoryRequest request);
    Task DeleteAsync(Guid id);
}
