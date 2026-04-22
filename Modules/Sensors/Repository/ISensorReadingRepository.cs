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

    /// <summary>
    /// Gets the latest reading per sensor (by max TimestampUtc) for the given sensor IDs.
    /// Used for health aggregation.
    /// </summary>
    /// <param name="sensorIds">Sensor IDs to fetch latest reading for</param>
    /// <returns>At most one reading per sensor (the most recent)</returns>
    Task<IReadOnlyList<Shared.Entity.SensorReading>> GetLatestBySensorIdsAsync(IReadOnlyList<Guid> sensorIds);

    /// <summary>
    /// Gets the count of readings for a sensor within a time window (for expected vs actual).
    /// </summary>
    /// <param name="sensorId">Sensor ID</param>
    /// <param name="from">Start of window (UTC inclusive)</param>
    /// <param name="to">End of window (UTC inclusive)</param>
    /// <returns>Number of readings in the window</returns>
    Task<int> CountBySensorIdInWindowAsync(Guid sensorId, DateTime from, DateTime to);

    /// <summary>
    /// Gets reading counts per sensor in a time window (for reliability batch computation).
    /// </summary>
    /// <param name="sensorIds">Sensor IDs</param>
    /// <param name="from">Start of window (UTC inclusive)</param>
    /// <param name="to">End of window (UTC inclusive)</param>
    /// <returns>Map of sensor ID to count of readings in the window</returns>
    Task<IReadOnlyDictionary<Guid, int>> GetReadingCountsBySensorIdsInWindowAsync(
        IReadOnlyList<Guid> sensorIds,
        DateTime from,
        DateTime to);
}
