using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public interface IShipmentLineRepository
{
    Task<(IReadOnlyList<IntakeShipmentLine> Items, int TotalCount)> GetPagedAsync(PaginationRequest request, Guid? shipmentId);
    Task<IntakeShipmentLine?> GetByIdAsync(Guid id);
    Task<IntakeShipmentLine> CreateAsync(IntakeShipmentLine entity);
    Task<IntakeShipmentLine> UpdateAsync(IntakeShipmentLine entity);
    Task<bool> DeleteAsync(Guid id);
}
