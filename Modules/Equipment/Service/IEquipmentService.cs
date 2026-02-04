using Modules.Equipment.Dto;

namespace Modules.Equipment.Service;

/// <summary>
/// Service interface for equipment operations
/// </summary>
public interface IEquipmentService
{
    /// <summary>
    /// Gets all equipment
    /// </summary>
    /// <returns>List of equipment responses</returns>
    Task<List<EquipmentResponse>> GetAllAsync();

    /// <summary>
    /// Gets equipment by ID
    /// </summary>
    /// <param name="id">Equipment ID</param>
    /// <returns>Equipment response</returns>
    /// <exception cref="KeyNotFoundException">When equipment is not found</exception>
    Task<EquipmentResponse> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates new equipment
    /// </summary>
    /// <param name="request">Equipment creation request</param>
    /// <returns>Created equipment response</returns>
    /// <exception cref="InvalidOperationException">When equipment creation fails</exception>
    Task<EquipmentResponse> CreateAsync(EquipmentRequest request);

    /// <summary>
    /// Updates existing equipment
    /// </summary>
    /// <param name="id">Equipment ID</param>
    /// <param name="request">Equipment update request</param>
    /// <returns>Updated equipment response</returns>
    /// <exception cref="KeyNotFoundException">When equipment is not found</exception>
    /// <exception cref="InvalidOperationException">When equipment update fails</exception>
    Task<EquipmentResponse> UpdateAsync(Guid id, EquipmentRequest request);

    /// <summary>
    /// Soft deletes equipment
    /// </summary>
    /// <param name="id">Equipment ID</param>
    /// <exception cref="KeyNotFoundException">When equipment is not found</exception>
    Task DeleteAsync(Guid id);
}
