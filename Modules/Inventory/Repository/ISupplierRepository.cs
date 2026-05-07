using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public interface ISupplierRepository
{
    Task<(IReadOnlyList<Supplier> Items, int TotalCount)> GetPagedAsync(PaginationRequest request);
    Task<Supplier?> GetByIdAsync(Guid id);
    Task<Supplier> CreateAsync(Supplier entity);
    Task<Supplier> UpdateAsync(Supplier entity);
    Task<bool> DeleteAsync(Guid id);
}
