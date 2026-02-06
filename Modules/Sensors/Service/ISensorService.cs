using Modules.Sensors.Dto;
using Shared.Dto;

namespace Modules.Sensors.Service;

/// <summary>
/// Service interface for sensor operations
/// </summary>
public interface ISensorService
{
    /// <summary>
    /// Gets paginated sensors
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated sensors and total count</returns>
    Task<(IReadOnlyList<Shared.Entity.Sensor> Items, int TotalCount)> GetAllAsync(PaginationRequest request);

    /// <summary>
    /// Gets a sensor by ID
    /// </summary>
    /// <param name="id">Sensor ID</param>
    /// <returns>Sensor entity</returns>
    /// <exception cref="KeyNotFoundException">When sensor is not found</exception>
    Task<Shared.Entity.Sensor> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new sensor
    /// </summary>
    /// <param name="request">Sensor creation request</param>
    /// <returns>Created sensor entity</returns>
    /// <exception cref="InvalidOperationException">When sensor creation fails</exception>
    Task<Shared.Entity.Sensor> CreateAsync(SensorRequest request);

    /// <summary>
    /// Updates an existing sensor
    /// </summary>
    /// <param name="id">Sensor ID</param>
    /// <param name="request">Sensor update request</param>
    /// <returns>Updated sensor entity</returns>
    /// <exception cref="KeyNotFoundException">When sensor is not found</exception>
    /// <exception cref="InvalidOperationException">When sensor update fails</exception>
    Task<Shared.Entity.Sensor> UpdateAsync(Guid id, SensorRequest request);

    /// <summary>
    /// Soft deletes a sensor
    /// </summary>
    /// <param name="id">Sensor ID</param>
    /// <exception cref="KeyNotFoundException">When sensor is not found</exception>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Gets paginated readings for a sensor
    /// </summary>
    /// <param name="sensorId">Sensor ID</param>
    /// <param name="request">Query parameters (pagination + time range)</param>
    /// <returns>Readings and total count</returns>
    /// <exception cref="KeyNotFoundException">When sensor is not found</exception>
    Task<(IReadOnlyList<Shared.Entity.SensorReading> Items, int TotalCount)> GetReadingsAsync(
        Guid sensorId,
        GetSensorReadingsRequest request);
}
