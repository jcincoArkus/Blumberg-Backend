using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Modules.Sensors.Repository;

/// <summary>
/// Repository implementation for querying sensor readings
/// </summary>
public class SensorReadingRepository(ApplicationDbContext context, ILogger<SensorReadingRepository> logger) : ISensorReadingRepository
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<Shared.Entity.SensorReading> Items, int TotalCount)> GetBySensorIdAsync(
        Guid sensorId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize)
    {
        logger.LogDebug("Querying sensor readings for sensor {SensorId}, page {Page}, pageSize {PageSize}", sensorId, page, pageSize);

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

        logger.LogInformation("Retrieved {Count} sensor readings from database (total: {TotalCount})", items.Count, totalCount);

        return (items, totalCount);
    }
}
