using Shared.Dto;

namespace Modules.Thresholds.Repository;

/// <summary>
/// Repository interface for Threshold entity operations
/// </summary>
public interface IThresholdRepository
{
    Task<(IReadOnlyList<Shared.Entity.Threshold> Items, int TotalCount)> GetPagedAsync(PaginationRequest request);
    Task<Shared.Entity.Threshold?> GetByIdAsync(Guid id);
    Task<Shared.Entity.Threshold> CreateAsync(Shared.Entity.Threshold entity);
    Task<Shared.Entity.Threshold> UpdateAsync(Shared.Entity.Threshold entity);
    Task<bool> SoftDeleteAsync(Guid id);
}
