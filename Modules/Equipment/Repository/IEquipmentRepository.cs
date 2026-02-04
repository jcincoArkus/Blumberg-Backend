using Shared.Entity;

namespace Modules.Equipment.Repository;

/// <summary>
/// Repository interface for Equipment entity operations
/// </summary>
public interface IEquipmentRepository
{
    /// <summary>
    /// Gets all equipment that are not soft deleted
    /// </summary>
    /// <returns>List of active equipment</returns>
    Task<List<Shared.Entity.Equipment>> GetAllAsync();

    /// <summary>
    /// Gets equipment by ID
    /// </summary>
    /// <param name="id">Equipment ID</param>
    /// <returns>Equipment entity or null if not found</returns>
    Task<Shared.Entity.Equipment?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates new equipment
    /// </summary>
    /// <param name="equipment">Equipment entity to create</param>
    /// <returns>Created equipment entity</returns>
    Task<Shared.Entity.Equipment> CreateAsync(Shared.Entity.Equipment equipment);

    /// <summary>
    /// Updates existing equipment
    /// </summary>
    /// <param name="equipment">Equipment entity to update</param>
    /// <returns>Updated equipment entity</returns>
    Task<Shared.Entity.Equipment> UpdateAsync(Shared.Entity.Equipment equipment);

    /// <summary>
    /// Soft deletes equipment by setting DeletedAt timestamp
    /// </summary>
    /// <param name="id">Equipment ID</param>
    /// <returns>True if equipment was found and deleted, false otherwise</returns>
    Task<bool> SoftDeleteAsync(Guid id);
}
