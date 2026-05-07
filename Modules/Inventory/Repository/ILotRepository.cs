using Modules.Inventory.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public interface ILotRepository
{
    Task<(IReadOnlyList<Lot> Items, int TotalCount)> GetPagedAsync(GetLotsRequest request);
    Task<Lot?> GetByCodeAsync(string lotCode);
    Task<Lot> CreateAsync(Lot entity);
    Task<Lot> UpdateAsync(Lot entity);
    Task<bool> DeleteAsync(string lotCode);
    Task<bool> ExistsByCodeAsync(string lotCode);
}
