using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public class IntakeShipmentRepository(ApplicationDbContext context, ILogger<IntakeShipmentRepository> logger) : IIntakeShipmentRepository
{
    [Span]
    public virtual async Task<(IReadOnlyList<IntakeShipment> Items, int TotalCount)> GetPagedAsync(GetIntakeShipmentsRequest request)
    {
        IQueryable<IntakeShipment> query = context.IntakeShipments
            .Include(e => e.Supplier)
            .Include(e => e.Site);

        if (request.SupplierId.HasValue)
            query = query.Where(e => e.SupplierId == request.SupplierId.Value);

        if (request.SiteId.HasValue)
            query = query.Where(e => e.SiteId == request.SiteId.Value);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(e => e.Status == request.Status);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(e => e.PoReference.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.ArrivedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} intake shipments (total: {TotalCount})", items.Count, totalCount);
        return (items, totalCount);
    }

    [Span]
    public virtual async Task<IntakeShipment?> GetByIdAsync(Guid id)
    {
        return await context.IntakeShipments
            .Include(e => e.Supplier)
            .Include(e => e.Site)
            .Include(e => e.Lines).ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<IntakeShipment> CreateAsync(IntakeShipment entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        context.IntakeShipments.Add(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Created intake shipment {Id} ({PoReference})", entity.Id, entity.PoReference);
        return entity;
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<IntakeShipment> UpdateAsync(IntakeShipment entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.IntakeShipments.Update(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Updated intake shipment {Id}", entity.Id);
        return entity;
    }

    [Span]
    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await context.IntakeShipments.FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null) return false;
        context.IntakeShipments.Remove(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Deleted intake shipment {Id}", id);
        return true;
    }
}
