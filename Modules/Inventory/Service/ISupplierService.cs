using Modules.Inventory.Dto;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Service;

public interface ISupplierService
{
    Task<(IReadOnlyList<Supplier> Items, int TotalCount)> GetAllAsync(PaginationRequest request);
    Task<Supplier> GetByIdAsync(Guid id);
    Task<Supplier> CreateAsync(SupplierRequest request);
    Task<Supplier> UpdateAsync(Guid id, SupplierRequest request);
    Task DeleteAsync(Guid id);
}
