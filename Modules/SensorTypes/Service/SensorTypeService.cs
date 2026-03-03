using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.SensorTypes.Dto;
using Modules.SensorTypes.Repository;
using Shared.Dto;
using Shared.Entity;
using Shared.Enums;

namespace Modules.SensorTypes.Service;

/// <summary>
/// Service implementation for sensor type operations
/// </summary>
public class SensorTypeService(ISensorTypeRepository repository, ILogger<SensorTypeService> logger) : ISensorTypeService
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<SensorType> Items, int TotalCount)> GetAllAsync(PaginationRequest request)
    {
        return await repository.GetPagedAsync(request);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<SensorType> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"Sensor type with ID {id} was not found");
        return entity;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<SensorType> CreateAsync(SensorTypeRequest request)
    {
        var entity = new SensorType
        {
            Type = request.Type,
            Unit = request.Unit
        };
        return await repository.CreateAsync(entity);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<SensorType> UpdateAsync(Guid id, SensorTypeRequest request)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"Sensor type with ID {id} was not found");
        entity.Type = request.Type;
        entity.Unit = request.Unit;
        return await repository.UpdateAsync(entity);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task DeleteAsync(Guid id)
    {
        var deleted = await repository.SoftDeleteAsync(id);
        if (!deleted)
            throw new KeyNotFoundException($"Sensor type with ID {id} was not found");
    }
}
