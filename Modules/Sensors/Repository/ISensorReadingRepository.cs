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
    /// <param name="fromUtc">Optional start of time range (inclusive)</param>
    /// <param name="toUtc">Optional end of time range (inclusive)</param>
    /// <param name="page">1-based page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Readings and total count</returns>
    Task<(IReadOnlyList<Shared.Entity.SensorReading> Items, int TotalCount)> GetBySensorIdAsync(
        Guid sensorId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize);
}
