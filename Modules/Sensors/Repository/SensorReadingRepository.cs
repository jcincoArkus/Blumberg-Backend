using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Sensors.Dto;

namespace Modules.Sensors.Repository;

/// <summary>
/// Repository implementation for querying sensor readings
/// </summary>
public class SensorReadingRepository(ApplicationDbContext context, ILogger<SensorReadingRepository> logger) : ISensorReadingRepository
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<Shared.Entity.SensorReading> Items, int TotalCount)> GetBySensorIdAsync(
        Guid sensorId,
        GetSensorReadingsRequest request)
    {
        logger.LogDebug("Querying sensor readings for sensor {SensorId}, page {Page}, pageSize {PageSize}", sensorId, request.Page, request.PageSize);

        var query = context.SensorReadings
            .Where(r => r.SensorId == sensorId);

        if (request.From.HasValue)
            query = query.Where(r => r.TimestampUtc >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(r => r.TimestampUtc <= request.To.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.TimestampUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} sensor readings from database (total: {TotalCount})", items.Count, totalCount);

        return (items, totalCount);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<IReadOnlyList<Shared.Entity.SensorReading>> GetLatestBySensorIdsAsync(IReadOnlyList<Guid> sensorIds)
    {
        if (sensorIds.Count == 0)
            return [];

        logger.LogDebug("Querying latest reading per sensor for {Count} sensors", sensorIds.Count);

        var allRecent = await context.SensorReadings
            .Include(r => r.IngestionRun)
            .Where(r => sensorIds.Contains(r.SensorId))
            .OrderByDescending(r => r.TimestampUtc)
            .ToListAsync();

        var seen = new HashSet<Guid>();
        var latest = new List<Shared.Entity.SensorReading>();
        foreach (var r in allRecent)
        {
            if (seen.Add(r.SensorId))
                latest.Add(r);
        }

        logger.LogInformation("Retrieved latest readings for {Count} sensors", latest.Count);
        return latest;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<int> CountBySensorIdInWindowAsync(Guid sensorId, DateTime from, DateTime to)
    {
        return await context.SensorReadings
            .Where(r => r.SensorId == sensorId && r.TimestampUtc >= from && r.TimestampUtc <= to)
            .CountAsync();
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<IReadOnlyDictionary<Guid, int>> GetReadingCountsBySensorIdsInWindowAsync(
        IReadOnlyList<Guid> sensorIds,
        DateTime from,
        DateTime to)
    {
        if (sensorIds.Count == 0)
            return new Dictionary<Guid, int>();

        var counts = await context.SensorReadings
            .Where(r => sensorIds.Contains(r.SensorId) && r.TimestampUtc >= from && r.TimestampUtc <= to)
            .GroupBy(r => r.SensorId)
            .Select(g => new { SensorId = g.Key, Count = g.Count() })
            .ToListAsync();

        return counts.ToDictionary(x => x.SensorId, x => x.Count);
    }
}
