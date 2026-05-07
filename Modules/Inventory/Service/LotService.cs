using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Modules.Inventory.Repository;
using Shared.Entity;

namespace Modules.Inventory.Service;

public class LotService(ILotRepository repository, ILogger<LotService> logger) : ILotService
{
    [Span]
    public virtual async Task<(IReadOnlyList<Lot> Items, int TotalCount)> GetAllAsync(GetLotsRequest request)
    {
        logger.LogDebug("Getting lots page {Page}", request.Page);
        return await repository.GetPagedAsync(request);
    }

    [Span]
    public virtual async Task<Lot> GetByCodeAsync(string lotCode)
    {
        var entity = await repository.GetByCodeAsync(lotCode);
        if (entity == null)
        {
            logger.LogWarning("Lot not found: {LotCode}", lotCode);
            throw new KeyNotFoundException($"Lot with code '{lotCode}' was not found");
        }
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<Lot> CreateAsync(LotRequest request)
    {
        if (await repository.ExistsByCodeAsync(request.LotCode))
            throw new InvalidOperationException($"A lot with code '{request.LotCode}' already exists");

        if (request.ExpiresAt <= request.EntryAt)
            throw new InvalidOperationException("ExpiresAt must be after EntryAt");

        var entity = new Lot
        {
            LotCode = request.LotCode,
            ProductId = request.ProductId,
            Qty = request.Qty,
            Unit = request.Unit,
            EntryAt = request.EntryAt,
            ExpiresAt = request.ExpiresAt,
            SiteId = request.SiteId,
            Zone = request.Zone,
            SupplierId = request.SupplierId,
            CostPerUnit = request.CostPerUnit
        };
        return await repository.CreateAsync(entity);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<Lot> UpdateAsync(string lotCode, LotRequest request)
    {
        var entity = await GetByCodeAsync(lotCode);

        if (request.ExpiresAt <= request.EntryAt)
            throw new InvalidOperationException("ExpiresAt must be after EntryAt");

        entity.ProductId = request.ProductId;
        entity.Qty = request.Qty;
        entity.Unit = request.Unit;
        entity.EntryAt = request.EntryAt;
        entity.ExpiresAt = request.ExpiresAt;
        entity.SiteId = request.SiteId;
        entity.Zone = request.Zone;
        entity.SupplierId = request.SupplierId;
        entity.CostPerUnit = request.CostPerUnit;
        return await repository.UpdateAsync(entity);
    }

    [Span]
    public virtual async Task DeleteAsync(string lotCode)
    {
        var deleted = await repository.DeleteAsync(lotCode);
        if (!deleted)
        {
            logger.LogWarning("Lot not found for deletion: {LotCode}", lotCode);
            throw new KeyNotFoundException($"Lot with code '{lotCode}' was not found");
        }
    }
}
