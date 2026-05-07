using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public interface IInventorySiteRepository
{
    Task<(IReadOnlyList<InventorySite> Items, int TotalCount)> GetPagedAsync(PaginationRequest request);
    Task<InventorySite?> GetByIdAsync(Guid id);
    Task<InventorySite> CreateAsync(InventorySite entity);
    Task<InventorySite> UpdateAsync(InventorySite entity);
    Task<bool> DeleteAsync(Guid id);
}
