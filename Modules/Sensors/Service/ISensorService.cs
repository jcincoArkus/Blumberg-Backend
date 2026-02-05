using Modules.Sensors.Dto;

namespace Modules.Sensors.Service;

/// <summary>
/// Service interface for sensor operations
/// </summary>
public interface ISensorService
{
    /// <summary>
    /// Gets all sensors
    /// </summary>
    /// <returns>List of sensor responses</returns>
    Task<List<SensorResponse>> GetAllAsync();

    /// <summary>
    /// Gets a sensor by ID
    /// </summary>
    /// <param name="id">Sensor ID</param>
    /// <returns>Sensor response</returns>
    /// <exception cref="KeyNotFoundException">When sensor is not found</exception>
    Task<SensorResponse> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new sensor
    /// </summary>
    /// <param name="request">Sensor creation request</param>
    /// <returns>Created sensor response</returns>
    /// <exception cref="InvalidOperationException">When sensor creation fails</exception>
    Task<SensorResponse> CreateAsync(SensorRequest request);

    /// <summary>
    /// Updates an existing sensor
    /// </summary>
    /// <param name="id">Sensor ID</param>
    /// <param name="request">Sensor update request</param>
    /// <returns>Updated sensor response</returns>
    /// <exception cref="KeyNotFoundException">When sensor is not found</exception>
    /// <exception cref="InvalidOperationException">When sensor update fails</exception>
    Task<SensorResponse> UpdateAsync(Guid id, SensorRequest request);

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
    /// <param name="fromUtc">Optional start of time range (inclusive)</param>
    /// <param name="toUtc">Optional end of time range (inclusive)</param>
    /// <param name="page">1-based page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated readings</returns>
    /// <exception cref="KeyNotFoundException">When sensor is not found</exception>
    Task<PagedResponse<SensorReadingResponse>> GetReadingsAsync(
        Guid sensorId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize);
}
