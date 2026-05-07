using Modules.Inventory.Dto;
using Shared.Entity;

namespace Modules.Inventory.Service;

public interface ILotService
{
    Task<(IReadOnlyList<Lot> Items, int TotalCount)> GetAllAsync(GetLotsRequest request);
    Task<Lot> GetByCodeAsync(string lotCode);
    Task<Lot> CreateAsync(LotRequest request);
    Task<Lot> UpdateAsync(string lotCode, LotRequest request);
    Task DeleteAsync(string lotCode);
}
