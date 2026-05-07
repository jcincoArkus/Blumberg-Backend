using Modules.Inventory.Dto;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Service;

public interface IInventorySiteService
{
    Task<(IReadOnlyList<InventorySite> Items, int TotalCount)> GetAllAsync(PaginationRequest request);
    Task<InventorySite> GetByIdAsync(Guid id);
    Task<InventorySite> CreateAsync(InventorySiteRequest request);
    Task<InventorySite> UpdateAsync(Guid id, InventorySiteRequest request);
    Task DeleteAsync(Guid id);
}
