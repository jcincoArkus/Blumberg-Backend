namespace Modules.Sensors.Repository;

/// <summary>
/// Repository interface for Sensor entity operations
/// </summary>
public interface ISensorRepository
{
    /// <summary>
    /// Gets all sensors that are not soft deleted
    /// </summary>
    /// <returns>List of active sensors</returns>
    Task<List<Shared.Entity.Sensor>> GetAllAsync();

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
