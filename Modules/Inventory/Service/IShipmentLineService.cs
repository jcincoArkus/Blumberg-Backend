using Modules.Inventory.Dto;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Service;

public interface IShipmentLineService
{
    Task<(IReadOnlyList<IntakeShipmentLine> Items, int TotalCount)> GetAllAsync(PaginationRequest request, Guid? shipmentId);
    Task<IntakeShipmentLine> GetByIdAsync(Guid id);
    Task<IntakeShipmentLine> CreateAsync(ShipmentLineRequest request);
    Task<IntakeShipmentLine> UpdateAsync(Guid id, ShipmentLineRequest request);
    Task DeleteAsync(Guid id);
}
