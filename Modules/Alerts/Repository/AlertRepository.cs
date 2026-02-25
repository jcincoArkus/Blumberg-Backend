using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Alerts.Dto;
using Shared.Entity;
using Shared.Enums;

namespace Modules.Alerts.Repository;

/// <summary>
/// Repository implementation for alert operations
/// </summary>
public class AlertRepository(ApplicationDbContext context, ILogger<AlertRepository> logger) : IAlertRepository
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<Alert> Items, int TotalCount)> GetPagedAsync(GetAlertsRequest request)
    {
        IQueryable<Alert> query = context.Alerts.Where(e => e.DeletedAt == null);

        if (request.Status.HasValue)
            query = query.Where(e => e.Status == request.Status.Value);
        if (request.SensorId.HasValue)
            query = query.Where(e => e.SensorId == request.SensorId.Value);
        if (request.EquipmentId.HasValue)
            query = query.Where(e => e.EquipmentId == request.EquipmentId.Value);
        if (request.SiteId.HasValue)
            query = query.Where(e => e.SiteId == request.SiteId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.TriggeredAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} alerts (total: {TotalCount})", items.Count, totalCount);
        return (items, totalCount);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Alert?> GetByIdAsync(Guid id)
    {
        return await context.Alerts
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Alert?> GetActiveBySensorIdAsync(Guid sensorId)
    {
        return await context.Alerts
            .FirstOrDefaultAsync(e => e.SensorId == sensorId && e.Status == AlertStatus.Active && e.DeletedAt == null);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Alert> CreateAsync(Alert entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.DeletedAt = null;
        context.Alerts.Add(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Created alert {Id} for sensor {SensorId}", entity.Id, entity.SensorId);
        return entity;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Alert> UpdateAsync(Alert entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Alerts.Update(entity);
        await context.SaveChangesAsync();
        logger.LogInformation("Updated alert {Id}", entity.Id);
        return entity;
    }
}
