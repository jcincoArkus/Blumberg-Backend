using Modules.Sensors.Dto;
using Shared.Dto;

namespace Modules.Sensors.Repository;

/// <summary>
/// Repository interface for Sensor entity operations
/// </summary>
public interface ISensorRepository
{
    /// <summary>
    /// Gets paginated sensors
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated sensors and total count</returns>
    Task<(IReadOnlyList<Shared.Entity.Sensor> Items, int TotalCount)> GetPagedAsync(PaginationRequest request);

    /// <summary>
    /// Gets all sensors matching health list filters (site, equipment, status) for health aggregation.
    /// Does not apply healthStatus filter or pagination; caller computes health and paginates.
    /// </summary>
    /// <param name="request">Filters: SiteId, EquipmentId, Status (HealthStatus and pagination ignored)</param>
    /// <returns>Matching sensors with Equipment and SensorType included</returns>
    Task<IReadOnlyList<Shared.Entity.Sensor>> GetForHealthListAsync(GetSensorHealthRequest request);

    /// <summary>
    /// Gets a sensor by ID
    /// </summary>
    /// <param name="id">Sensor ID</param>
    /// <returns>Sensor entity or null if not found</returns>
    Task<Shared.Entity.Sensor?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new sensor
    /// </summary>
    /// <param name="sensor">Sensor entity to create</param>
    /// <returns>Created sensor entity</returns>
    Task<Shared.Entity.Sensor> CreateAsync(Shared.Entity.Sensor sensor);

    /// <summary>
    /// Updates an existing sensor
    /// </summary>
    /// <param name="sensor">Sensor entity to update</param>
    /// <returns>Updated sensor entity</returns>
    Task<Shared.Entity.Sensor> UpdateAsync(Shared.Entity.Sensor sensor);

    /// <summary>
    /// Soft deletes a sensor by setting DeletedAt timestamp
    /// </summary>
    /// <param name="id">Sensor ID</param>
    /// <returns>True if sensor was found and deleted, false otherwise</returns>
    Task<bool> SoftDeleteAsync(Guid id);
}
