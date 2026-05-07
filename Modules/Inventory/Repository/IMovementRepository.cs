using Modules.Inventory.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public interface IMovementRepository
{
    Task<(IReadOnlyList<Movement> Items, int TotalCount)> GetPagedAsync(GetMovementsRequest request);
    Task<Movement?> GetByIdAsync(Guid id);
    Task<Movement> CreateAsync(Movement entity);
}
