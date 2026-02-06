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
}
