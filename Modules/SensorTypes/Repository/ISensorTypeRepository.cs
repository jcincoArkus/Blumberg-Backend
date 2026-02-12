using Shared.Dto;

namespace Modules.SensorTypes.Repository;

/// <summary>
/// Repository interface for SensorType entity operations
/// </summary>
public interface ISensorTypeRepository
{
    Task<(IReadOnlyList<Shared.Entity.SensorType> Items, int TotalCount)> GetPagedAsync(PaginationRequest request);
    Task<Shared.Entity.SensorType?> GetByIdAsync(Guid id);
    Task<Shared.Entity.SensorType> CreateAsync(Shared.Entity.SensorType entity);
    Task<Shared.Entity.SensorType> UpdateAsync(Shared.Entity.SensorType entity);
    Task<bool> SoftDeleteAsync(Guid id);
}
