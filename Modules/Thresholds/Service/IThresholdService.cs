using Modules.Thresholds.Dto;
using Shared.Dto;

namespace Modules.Thresholds.Service;

/// <summary>
/// Service interface for threshold operations
/// </summary>
public interface IThresholdService
{
    Task<(IReadOnlyList<Shared.Entity.Threshold> Items, int TotalCount)> GetAllAsync(PaginationRequest request);
    Task<Shared.Entity.Threshold> GetByIdAsync(Guid id);
    Task<Shared.Entity.Threshold> CreateAsync(ThresholdRequest request);
    Task<Shared.Entity.Threshold> UpdateAsync(Guid id, ThresholdRequest request);
    Task DeleteAsync(Guid id);
}
