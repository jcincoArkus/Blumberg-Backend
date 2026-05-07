using Modules.Inventory.Dto;
using Shared.Entity;

namespace Modules.Inventory.Service;

public interface IMovementService
{
    Task<(IReadOnlyList<Movement> Items, int TotalCount)> GetAllAsync(GetMovementsRequest request);
    Task<Movement> GetByIdAsync(Guid id);
    Task<Movement> CreateAsync(MovementRequest request);
}
