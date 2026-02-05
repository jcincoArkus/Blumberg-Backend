using Adapters.Database;
using Microsoft.EntityFrameworkCore;

namespace Modules.Sensors.Repository;

/// <summary>
/// Repository implementation for querying sensor readings
/// </summary>
public class SensorReadingRepository(ApplicationDbContext context) : ISensorReadingRepository
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Shared.Entity.SensorReading> Items, int TotalCount)> GetBySensorIdAsync(
        Guid sensorId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize)
    {
        var query = _context.SensorReadings
            .Where(r => r.SensorId == sensorId);

        if (fromUtc.HasValue)
            query = query.Where(r => r.TimestampUtc >= fromUtc.Value);
        if (toUtc.HasValue)
            query = query.Where(r => r.TimestampUtc <= toUtc.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.TimestampUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
