using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Modules.Inventory.Repository;
using Shared.Entity;

namespace Modules.Inventory.Service;

public class IntakeShipmentService(IIntakeShipmentRepository repository, ILogger<IntakeShipmentService> logger) : IIntakeShipmentService
{
    [Span]
    public virtual async Task<(IReadOnlyList<IntakeShipment> Items, int TotalCount)> GetAllAsync(GetIntakeShipmentsRequest request)
    {
        logger.LogDebug("Getting intake shipments page {Page}", request.Page);
        return await repository.GetPagedAsync(request);
    }

    [Span]
    public virtual async Task<IntakeShipment> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
        {
            logger.LogWarning("Intake shipment not found: {Id}", id);
            throw new KeyNotFoundException($"Intake shipment with ID {id} was not found");
        }
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<IntakeShipment> CreateAsync(IntakeShipmentRequest request)
    {
        var entity = new IntakeShipment
        {
            PoReference = request.PoReference,
            SupplierId = request.SupplierId,
            Vehicle = request.Vehicle,
            Driver = request.Driver,
            SiteId = request.SiteId,
            ReceivingZone = request.ReceivingZone,
            ColdChainTempC = request.ColdChainTempC,
            ArrivedAt = request.ArrivedAt,
            ReceivedBy = request.ReceivedBy,
            Status = request.Status
        };
        return await repository.CreateAsync(entity);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<IntakeShipment> UpdateAsync(Guid id, IntakeShipmentRequest request)
    {
        var entity = await GetByIdAsync(id);
        entity.PoReference = request.PoReference;
        entity.SupplierId = request.SupplierId;
        entity.Vehicle = request.Vehicle;
        entity.Driver = request.Driver;
        entity.SiteId = request.SiteId;
        entity.ReceivingZone = request.ReceivingZone;
        entity.ColdChainTempC = request.ColdChainTempC;
        entity.ArrivedAt = request.ArrivedAt;
        entity.ReceivedBy = request.ReceivedBy;
        entity.Status = request.Status;
        return await repository.UpdateAsync(entity);
    }

    [Span]
    public virtual async Task DeleteAsync(Guid id)
    {
        var deleted = await repository.DeleteAsync(id);
        if (!deleted)
        {
            logger.LogWarning("Intake shipment not found for deletion: {Id}", id);
            throw new KeyNotFoundException($"Intake shipment with ID {id} was not found");
        }
    }
}
