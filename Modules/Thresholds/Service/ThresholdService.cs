using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Thresholds.Dto;
using Modules.Thresholds.Repository;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Thresholds.Service;

/// <summary>
/// Service implementation for threshold operations
/// </summary>
public class ThresholdService(IThresholdRepository repository, ILogger<ThresholdService> logger) : IThresholdService
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<Threshold> Items, int TotalCount)> GetAllAsync(PaginationRequest request)
    {
        return await repository.GetPagedAsync(request);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Threshold> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"Threshold with ID {id} was not found");
        return entity;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Threshold> CreateAsync(ThresholdRequest request)
    {
        var entity = new Threshold
        {
            Min = request.Min,
            Max = request.Max,
            Duration = TimeSpan.FromSeconds(request.DurationSeconds)
        };
        return await repository.CreateAsync(entity);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Threshold> UpdateAsync(Guid id, ThresholdRequest request)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"Threshold with ID {id} was not found");
        entity.Min = request.Min;
        entity.Max = request.Max;
        entity.Duration = TimeSpan.FromSeconds(request.DurationSeconds);
        return await repository.UpdateAsync(entity);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task DeleteAsync(Guid id)
    {
        var deleted = await repository.SoftDeleteAsync(id);
        if (!deleted)
            throw new KeyNotFoundException($"Threshold with ID {id} was not found");
    }
}
