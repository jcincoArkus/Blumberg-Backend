using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public class ShipmentLineRepository(ApplicationDbContext context, ILogger<ShipmentLineRepository> logger) : IShipmentLineRepository
{
    [Span]
    public virtual async Task<(IReadOnlyList<IntakeShipmentLine> Items, int TotalCount)> GetPagedAsync(PaginationRequest request, Guid? shipmentId)
    {
        IQueryable<IntakeShipmentLine> query = context.IntakeShipmentLines
            .Include(e => e.Shipment)
            .Include(e => e.Product);

        if (shipmentId.HasValue)
            query = query.Where(e => e.ShipmentId == shipmentId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} shipment lines (total: {TotalCount})", items.Count, totalCount);
        return (items, totalCount);
    }

    [Span]
    public virtual async Task<IntakeShipmentLine?> GetByIdAsync(Guid id)
    {
        return await context.IntakeShipmentLines
            .Include(e => e.Shipment)
            .Include(e => e.Product)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<IntakeShipmentLine> CreateAsync(IntakeShipmentLine entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        context.IntakeShipmentLines.Add(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Created shipment line {Id}", entity.Id);
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<IntakeShipmentLine> UpdateAsync(IntakeShipmentLine entity)
    {
        context.IntakeShipmentLines.Update(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Updated shipment line {Id}", entity.Id);
        return entity;
    }

    [Span]
    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await context.IntakeShipmentLines.FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null) return false;
        context.IntakeShipmentLines.Remove(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Deleted shipment line {Id}", id);
        return true;
    }
}
