using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Modules.Inventory.Repository;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Service;

public class ShipmentLineService(IShipmentLineRepository repository, ILogger<ShipmentLineService> logger) : IShipmentLineService
{
    [Span]
    public virtual async Task<(IReadOnlyList<IntakeShipmentLine> Items, int TotalCount)> GetAllAsync(PaginationRequest request, Guid? shipmentId)
    {
        logger.LogDebug("Getting shipment lines, shipmentId={ShipmentId}", shipmentId);
        return await repository.GetPagedAsync(request, shipmentId);
    }

    [Span]
    public virtual async Task<IntakeShipmentLine> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
        {
            logger.LogWarning("Shipment line not found: {Id}", id);
            throw new KeyNotFoundException($"Shipment line with ID {id} was not found");
        }
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<IntakeShipmentLine> CreateAsync(ShipmentLineRequest request)
    {
        var entity = new IntakeShipmentLine
        {
            ShipmentId = request.ShipmentId,
            ProductId = request.ProductId,
            LotCode = request.LotCode,
            Qty = request.Qty,
            Unit = request.Unit,
            CostPerUnit = request.CostPerUnit
        };
        return await repository.CreateAsync(entity);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<IntakeShipmentLine> UpdateAsync(Guid id, ShipmentLineRequest request)
    {
        var entity = await GetByIdAsync(id);
        entity.ShipmentId = request.ShipmentId;
        entity.ProductId = request.ProductId;
        entity.LotCode = request.LotCode;
        entity.Qty = request.Qty;
        entity.Unit = request.Unit;
        entity.CostPerUnit = request.CostPerUnit;
        return await repository.UpdateAsync(entity);
    }

    [Span]
    public virtual async Task DeleteAsync(Guid id)
    {
        var deleted = await repository.DeleteAsync(id);
        if (!deleted)
        {
            logger.LogWarning("Shipment line not found for deletion: {Id}", id);
            throw new KeyNotFoundException($"Shipment line with ID {id} was not found");
        }
    }
}
