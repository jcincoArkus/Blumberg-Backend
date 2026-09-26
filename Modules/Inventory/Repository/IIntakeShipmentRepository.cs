using Modules.Inventory.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public interface IIntakeShipmentRepository
{
    Task<(IReadOnlyList<IntakeShipment> Items, int TotalCount)> GetPagedAsync(GetIntakeShipmentsRequest request);
    Task<IntakeShipment?> GetByIdAsync(Guid id);
    Task<IntakeShipment> CreateAsync(IntakeShipment entity);
    Task<IntakeShipment> UpdateAsync(IntakeShipment entity);
    Task<bool> DeleteAsync(Guid id);
}
