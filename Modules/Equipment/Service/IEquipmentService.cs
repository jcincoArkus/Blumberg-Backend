using Modules.Equipment.Dto;
using Shared.Dto;

namespace Modules.Equipment.Service;

/// <summary>
/// Service interface for equipment operations
/// </summary>
public interface IEquipmentService
{
    /// <summary>
    /// Gets paginated equipment
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated equipment and total count</returns>
    Task<(IReadOnlyList<Shared.Entity.Equipment> Items, int TotalCount)> GetAllAsync(PaginationRequest request);

    /// <summary>
    /// Gets equipment by ID
    /// </summary>
    /// <param name="id">Equipment ID</param>
    /// <returns>Equipment entity</returns>
    /// <exception cref="KeyNotFoundException">When equipment is not found</exception>
    Task<Shared.Entity.Equipment> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates new equipment
    /// </summary>
    /// <param name="request">Equipment creation request</param>
    /// <returns>Created equipment entity</returns>
    /// <exception cref="InvalidOperationException">When equipment creation fails</exception>
    Task<Shared.Entity.Equipment> CreateAsync(EquipmentRequest request);

    /// <summary>
    /// Updates existing equipment
    /// </summary>
    /// <param name="id">Equipment ID</param>
    /// <param name="request">Equipment update request</param>
    /// <returns>Updated equipment entity</returns>
    /// <exception cref="KeyNotFoundException">When equipment is not found</exception>
    /// <exception cref="InvalidOperationException">When equipment update fails</exception>
    Task<Shared.Entity.Equipment> UpdateAsync(Guid id, EquipmentRequest request);

    /// <summary>
    /// Soft deletes equipment
    /// </summary>
    /// <param name="id">Equipment ID</param>
    /// <exception cref="KeyNotFoundException">When equipment is not found</exception>
    Task DeleteAsync(Guid id);
}
