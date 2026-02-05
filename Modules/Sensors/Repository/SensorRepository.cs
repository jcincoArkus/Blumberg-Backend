using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Modules.Sensors.Repository;

/// <summary>
/// Repository implementation for Sensor entity operations
/// </summary>
public class SensorRepository(ApplicationDbContext context, ILogger<SensorRepository> logger) : ISensorRepository
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc />
    [Span]
    public virtual async Task<List<Shared.Entity.Sensor>> GetAllAsync()
    {
        logger.LogDebug("Querying all sensors");

        var sensors = await _context.Sensors
            .Where(s => s.DeletedAt == null)
            .Include(s => s.Organization)
            .Include(s => s.Equipment)
            .Include(s => s.SensorType)
            .Include(s => s.Threshold)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} sensors from database", sensors.Count);

        return sensors;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Shared.Entity.Sensor?> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Querying sensor by ID: {Id}", id);

        var sensor = await _context.Sensors
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

        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();

        logger.LogInformation("Sensor created in database with ID: {Id}", sensor.Id);

        return sensor;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Shared.Entity.Sensor> UpdateAsync(Shared.Entity.Sensor sensor)
    {
        logger.LogDebug("Updating sensor in database: {Id}", sensor.Id);

        sensor.UpdatedAt = DateTime.UtcNow;

        _context.Sensors.Update(sensor);
        await _context.SaveChangesAsync();

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

        await _context.SaveChangesAsync();

        logger.LogInformation("Sensor {Id} soft deleted in database", id);

        return true;
    }
}
