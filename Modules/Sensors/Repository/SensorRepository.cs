using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Sensors.Dto;
using Shared.Dto;

namespace Modules.Sensors.Repository;

/// <summary>
/// Repository implementation for Sensor entity operations
/// </summary>
public class SensorRepository(ApplicationDbContext context, ILogger<SensorRepository> logger) : ISensorRepository
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<Shared.Entity.Sensor> Items, int TotalCount)> GetPagedAsync(PaginationRequest request)
    {
        logger.LogDebug("Querying sensors page {Page}, pageSize {PageSize}, search '{Search}'", request.Page, request.PageSize, request.Search);

        IQueryable<Shared.Entity.Sensor> query = context.Sensors
            .Where(s => s.DeletedAt == null)
            .Include(s => s.Organization)
            .Include(s => s.Equipment)
            .Include(s => s.SensorType)
            .Include(s => s.Threshold);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(s => s.Serial.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(s => s.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} sensors from database (total: {TotalCount})", items.Count, totalCount);

        return (items, totalCount);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<IReadOnlyList<Shared.Entity.Sensor>> GetForHealthListAsync(GetSensorHealthRequest request)
    {
        IQueryable<Shared.Entity.Sensor> query = context.Sensors
            .Where(s => s.DeletedAt == null)
            .Include(s => s.Organization)
            .Include(s => s.Equipment)
                .ThenInclude(e => e.Site)
            .Include(s => s.SensorType)
            .Include(s => s.Threshold);

        if (request.SiteId.HasValue)
            query = query.Where(s => s.Equipment.SiteId == request.SiteId.Value);
        if (request.EquipmentId.HasValue)
            query = query.Where(s => s.EquipmentId == request.EquipmentId.Value);
        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(s => s.Serial.ToLower().Contains(search));
        }

        var items = await query.OrderBy(s => s.CreatedAt).ToListAsync();
        logger.LogDebug("Retrieved {Count} sensors for health list (filters applied)", items.Count);
        return items;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Shared.Entity.Sensor?> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Querying sensor by ID: {Id}", id);

        var sensor = await context.Sensors
            .Include(s => s.Organization)
            .Include(s => s.Equipment)
            .Include(s => s.SensorType)
            .Include(s => s.Threshold)
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null);

        if (sensor != null)
            logger.LogInformation("Found sensor {Id}", id);
        else
            logger.LogDebug("Sensor not found with ID: {Id}", id);

        return sensor;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Shared.Entity.Sensor> CreateAsync(Shared.Entity.Sensor sensor)
    {
        logger.LogDebug("Creating sensor in database: {Serial}", sensor.Serial);

        sensor.Id = Guid.NewGuid();
        sensor.CreatedAt = DateTime.UtcNow;
        sensor.UpdatedAt = null;
        sensor.DeletedAt = null;

        context.Sensors.Add(sensor);
        await context.SaveChangesAsync();

        logger.LogInformation("Sensor created in database with ID: {Id}", sensor.Id);

        return sensor;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Shared.Entity.Sensor> UpdateAsync(Shared.Entity.Sensor sensor)
    {
        logger.LogDebug("Updating sensor in database: {Id}", sensor.Id);

        sensor.UpdatedAt = DateTime.UtcNow;

        context.Sensors.Update(sensor);
        await context.SaveChangesAsync();

        logger.LogInformation("Sensor {Id} updated in database", sensor.Id);

        return sensor;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<bool> SoftDeleteAsync(Guid id)
    {
        logger.LogDebug("Soft deleting sensor in database: {Id}", id);

        var sensor = await GetByIdAsync(id);
        if (sensor == null)
        {
            logger.LogWarning("Sensor not found for deletion: {Id}", id);
            return false;
        }

        sensor.DeletedAt = DateTime.UtcNow;
        sensor.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Sensor {Id} soft deleted in database", id);

        return true;
    }
}
