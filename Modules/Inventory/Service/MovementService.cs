using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Modules.Inventory.Repository;
using Shared.Entity;

namespace Modules.Inventory.Service;

public class MovementService(IMovementRepository repository, ILogger<MovementService> logger) : IMovementService
{
    [Span]
    public virtual async Task<(IReadOnlyList<Movement> Items, int TotalCount)> GetAllAsync(GetMovementsRequest request)
    {
        logger.LogDebug("Getting movements page {Page}", request.Page);
        return await repository.GetPagedAsync(request);
    }

    [Span]
    public virtual async Task<Movement> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
        {
            logger.LogWarning("Movement not found: {Id}", id);
            throw new KeyNotFoundException($"Movement with ID {id} was not found");
        }
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<Movement> CreateAsync(MovementRequest request)
    {
        if (request.Type == "transfer" && request.DestSiteId == null)
            throw new InvalidOperationException("DestSiteId is required for transfer movements");

        var entity = new Movement
        {
            Type = request.Type,
            OccurredAt = request.OccurredAt,
            ProductId = request.ProductId,
            Qty = request.Qty,
            Unit = request.Unit,
            LotCode = request.LotCode,
            SiteId = request.SiteId,
            DestSiteId = request.DestSiteId,
            PerformedBy = request.PerformedBy,
            Note = request.Note
        };
        return await repository.CreateAsync(entity);
    }
}
