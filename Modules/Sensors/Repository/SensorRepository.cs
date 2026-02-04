using Adapters.Database;
using Microsoft.EntityFrameworkCore;

namespace Modules.Sensors.Repository;

/// <summary>
/// Repository implementation for Sensor entity operations
/// </summary>
public class SensorRepository(ApplicationDbContext context) : ISensorRepository
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc />
    public async Task<List<Shared.Entity.Sensor>> GetAllAsync()
    {
        return await _context.Sensors
            .Where(s => s.DeletedAt == null)
            .Include(s => s.Organization)
            .Include(s => s.Equipment)
            .Include(s => s.SensorType)
            .Include(s => s.Threshold)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Shared.Entity.Sensor?> GetByIdAsync(Guid id)
    {
        return await _context.Sensors
            .Include(s => s.Organization)
            .Include(s => s.Equipment)
            .Include(s => s.SensorType)
            .Include(s => s.Threshold)
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null);
    }

    /// <inheritdoc />
    public async Task<Shared.Entity.Sensor> CreateAsync(Shared.Entity.Sensor sensor)
    {
        sensor.Id = Guid.NewGuid();
        sensor.CreatedAt = DateTime.UtcNow;
        sensor.UpdatedAt = null;
        sensor.DeletedAt = null;

        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();

        return sensor;
    }

    /// <inheritdoc />
    public async Task<Shared.Entity.Sensor> UpdateAsync(Shared.Entity.Sensor sensor)
    {
        sensor.UpdatedAt = DateTime.UtcNow;

        _context.Sensors.Update(sensor);
        await _context.SaveChangesAsync();

        return sensor;
    }

    /// <inheritdoc />
    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var sensor = await GetByIdAsync(id);
        if (sensor == null)
            return false;

        sensor.DeletedAt = DateTime.UtcNow;
        sensor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
