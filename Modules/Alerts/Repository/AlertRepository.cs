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
    public virtual async Task<(IReadOnlyList<AlertResponse> Items, int TotalCount)> GetPagedResponsesAsync(GetAlertsRequest request)
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
            .Select(a => new AlertResponse
            {
                Id = a.Id,
                SensorId = a.SensorId,
                EquipmentId = a.EquipmentId,
                SiteId = a.SiteId,
                Severity = a.Severity.ToString(),
                TriggeredValue = a.TriggeredValue,
                ThresholdMin = a.ThresholdMin,
                ThresholdMax = a.ThresholdMax,
                TriggeredAt = a.TriggeredAt,
                Status = a.Status.ToString(),
                ResolvedAt = a.ResolvedAt,
                CreatedAt = a.CreatedAt,
                EquipmentName = a.Equipment != null ? a.Equipment.Name : null,
                SensorSerial = a.Sensor != null ? a.Sensor.Serial : null,
                SensorTypeName = a.Sensor != null && a.Sensor.SensorType != null ? a.Sensor.SensorType.Type.ToString() : null,
            })
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} alerts (total: {TotalCount})", items.Count, totalCount);
        return (items, totalCount);
    }

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
            .Include(e => e.Sensor)
            .Include(e => e.Equipment)
            .OrderByDescending(e => e.TriggeredAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} alerts (total: {TotalCount})", items.Count, totalCount);
        return (items, totalCount);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<IReadOnlyList<ActiveAlertResponse>> GetActiveAsync()
    {
        var now = DateTime.UtcNow;

        var items = await context.Alerts
            .Where(e => e.DeletedAt == null
                && (e.Status == AlertStatus.Active || e.Status == AlertStatus.Acknowledged))
            .OrderBy(e => e.Severity)
            .ThenBy(e => e.TriggeredAt)
            .Select(a => new ActiveAlertResponse
            {
                Id = a.Id,
                Severity = a.Severity.ToString(),
                Status = a.Status.ToString(),
                SensorSerial = a.Sensor != null ? a.Sensor.Serial : null,
                SensorTypeName = a.Sensor != null && a.Sensor.SensorType != null ? a.Sensor.SensorType.Type.ToString() : null,
                EquipmentName = a.Equipment != null ? a.Equipment.Name : null,
                TriggeredAt = a.TriggeredAt,
            })
            .ToListAsync();

        foreach (var item in items)
            item.DurationSeconds = (now - item.TriggeredAt).TotalSeconds;

        logger.LogInformation("Retrieved {Count} active alerts", items.Count);
        return items;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Alert?> GetByIdAsync(Guid id)
    {
        return await context.Alerts
            .Include(e => e.Sensor).ThenInclude(s => s.SensorType)
            .Include(e => e.Equipment)
            .Include(a => a.Events)
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
    public virtual async Task<Alert?> GetUnresolvedBySensorIdAsync(Guid sensorId)
    {
        return await context.Alerts
            .FirstOrDefaultAsync(e =>
                e.SensorId == sensorId
                && (e.Status == AlertStatus.Active || e.Status == AlertStatus.Acknowledged)
                && e.DeletedAt == null);
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

        var triggeredEvent = new AlertEvent
        {
            Id = Guid.NewGuid(),
            AlertId = entity.Id,
            OrganizationId = entity.OrganizationId,
            EventType = AlertEventType.Triggered,
            OccurredAt = entity.TriggeredAt,
            Description = "Alert triggered",
            CreatedAt = DateTime.UtcNow,
        };
        context.AlertEvents.Add(triggeredEvent);

        await context.SaveChangesAsync();
        logger.LogInformation("Created alert {Id} for sensor {SensorId}", entity.Id, entity.SensorId);
        return entity;
    }

    /// <inheritdoc />
    [Span]
    public virtual void AddRangeWithTriggeredEvents(IEnumerable<Alert> entities)
    {
        var list = entities.ToList();
        foreach (var entity in list)
        {
            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();
            if (entity.CreatedAt == default)
                entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = null;
            entity.DeletedAt = null;
            context.Alerts.Add(entity);

            var triggeredEvent = new AlertEvent
            {
                Id = Guid.NewGuid(),
                AlertId = entity.Id,
                OrganizationId = entity.OrganizationId,
                EventType = AlertEventType.Triggered,
                OccurredAt = entity.TriggeredAt,
                Description = "Alert triggered",
                CreatedAt = DateTime.UtcNow,
            };
            context.AlertEvents.Add(triggeredEvent);
        }
        logger.LogDebug("Added {Count} alerts with Triggered events to context (no SaveChanges)", list.Count);
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

    /// <inheritdoc />
    [Span]
    public virtual async Task AddEventAsync(AlertEvent entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = null;
        entity.DeletedAt = null;
        context.AlertEvents.Add(entity);
        await context.SaveChangesAsync();
        logger.LogDebug("Added event {EventType} for alert {AlertId}", entity.EventType, entity.AlertId);
    }
}
