using Modules.Sensors.Dto;

namespace Modules.Sensors.Repository;

/// <summary>
/// Repository interface for querying sensor readings (time-series)
/// </summary>
public interface ISensorReadingRepository
{
    /// <summary>
    /// Gets paginated readings for a sensor, optionally filtered by time range.
    /// Uses indexes (SensorId, TimestampUtc) for efficient time-series queries.
    /// </summary>
    /// <param name="sensorId">Sensor ID</param>
    /// <param name="request">Query parameters (pagination + time range)</param>
    /// <returns>Readings and total count</returns>
    Task<(IReadOnlyList<Shared.Entity.SensorReading> Items, int TotalCount)> GetBySensorIdAsync(
        Guid sensorId,
        GetSensorReadingsRequest request);
}
