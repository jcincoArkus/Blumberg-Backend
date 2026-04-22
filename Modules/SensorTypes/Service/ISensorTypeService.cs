using Modules.SensorTypes.Dto;
using Shared.Dto;

namespace Modules.SensorTypes.Service;

/// <summary>
/// Service interface for sensor type operations
/// </summary>
public interface ISensorTypeService
{
    Task<(IReadOnlyList<Shared.Entity.SensorType> Items, int TotalCount)> GetAllAsync(PaginationRequest request);
    Task<Shared.Entity.SensorType> GetByIdAsync(Guid id);
    Task<Shared.Entity.SensorType> CreateAsync(SensorTypeRequest request);
    Task<Shared.Entity.SensorType> UpdateAsync(Guid id, SensorTypeRequest request);
    Task DeleteAsync(Guid id);
}
