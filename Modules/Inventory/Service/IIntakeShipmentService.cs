using Modules.Inventory.Dto;
using Shared.Entity;

namespace Modules.Inventory.Service;

public interface IIntakeShipmentService
{
    Task<(IReadOnlyList<IntakeShipment> Items, int TotalCount)> GetAllAsync(GetIntakeShipmentsRequest request);
    Task<IntakeShipment> GetByIdAsync(Guid id);
    Task<IntakeShipment> CreateAsync(IntakeShipmentRequest request);
    Task<IntakeShipment> UpdateAsync(Guid id, IntakeShipmentRequest request);
    Task DeleteAsync(Guid id);
}
