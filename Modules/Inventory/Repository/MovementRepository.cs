using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public class MovementRepository(ApplicationDbContext context, ILogger<MovementRepository> logger) : IMovementRepository
{
    [Span]
    public virtual async Task<(IReadOnlyList<Movement> Items, int TotalCount)> GetPagedAsync(GetMovementsRequest request)
    {
        IQueryable<Movement> query = context.Movements
            .Include(e => e.Product)
            .Include(e => e.Site)
            .Include(e => e.DestSite);

        if (!string.IsNullOrWhiteSpace(request.Type))
            query = query.Where(e => e.Type == request.Type);

        if (request.ProductId.HasValue)
            query = query.Where(e => e.ProductId == request.ProductId.Value);

        if (request.SiteId.HasValue)
            query = query.Where(e => e.SiteId == request.SiteId.Value);

        if (!string.IsNullOrWhiteSpace(request.LotCode))
            query = query.Where(e => e.LotCode == request.LotCode);

        if (request.From.HasValue)
            query = query.Where(e => e.OccurredAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(e => e.OccurredAt <= request.To.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} movements (total: {TotalCount})", items.Count, totalCount);
        return (items, totalCount);
    }

    [Span]
    public virtual async Task<Movement?> GetByIdAsync(Guid id)
    {
        return await context.Movements
            .Include(e => e.Product)
            .Include(e => e.Site)
            .Include(e => e.DestSite)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    [Span(IncludeArguments = true)]
    public virtual async Task<Movement> CreateAsync(Movement entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        context.Movements.Add(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Created movement {Id} ({Type})", entity.Id, entity.Type);
        return entity;
    }
}
