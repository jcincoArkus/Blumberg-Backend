using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Equipment.Dto;
using Modules.Equipment.Repository;
using Shared.Dto;

namespace Modules.Equipment.Service;

/// <summary>
/// Service implementation for equipment operations
/// </summary>
public class EquipmentService(IEquipmentRepository equipmentRepository, ILogger<EquipmentService> logger) : IEquipmentService
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<Shared.Entity.Equipment> Items, int TotalCount)> GetAllAsync(PaginationRequest request)
    {
        logger.LogDebug("Getting equipment page {Page}, pageSize {PageSize}", request.Page, request.PageSize);

        var result = await equipmentRepository.GetPagedAsync(request);

        logger.LogInformation("Retrieved {Count} equipment (total: {TotalCount})", result.Items.Count, result.TotalCount);

        return result;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Shared.Entity.Equipment> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Getting equipment by ID: {Id}", id);

        var equipment = await equipmentRepository.GetByIdAsync(id);

        if (equipment == null)
        {
            logger.LogWarning("Equipment not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Equipment with ID {id} was not found");
        }

        logger.LogInformation("Retrieved equipment {Id}", id);

        return equipment;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Shared.Entity.Equipment> CreateAsync(EquipmentRequest request)
    {
        logger.LogDebug("Creating new equipment: {Name}", request.Name);

        var equipment = new Shared.Entity.Equipment
        {
            Name = request.Name,
            EquipmentType = request.EquipmentType,
            SiteId = request.SiteId
        };

        var createdEquipment = await equipmentRepository.CreateAsync(equipment);

        logger.LogInformation("Equipment created successfully with ID: {Id}", createdEquipment.Id);

        return createdEquipment;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Shared.Entity.Equipment> UpdateAsync(Guid id, EquipmentRequest request)
    {
        logger.LogDebug("Updating equipment {Id}", id);

        var equipment = await equipmentRepository.GetByIdAsync(id);

        if (equipment == null)
        {
            logger.LogWarning("Equipment not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Equipment with ID {id} was not found");
        }

        equipment.Name = request.Name;
        equipment.EquipmentType = request.EquipmentType;
        equipment.SiteId = request.SiteId;

        var updatedEquipment = await equipmentRepository.UpdateAsync(equipment);

        logger.LogInformation("Equipment {Id} updated successfully", id);

        return updatedEquipment;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task DeleteAsync(Guid id)
    {
        logger.LogDebug("Deleting equipment {Id}", id);

        var deleted = await equipmentRepository.SoftDeleteAsync(id);

        if (!deleted)
        {
            logger.LogWarning("Equipment not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Equipment with ID {id} was not found");
        }

        logger.LogInformation("Equipment {Id} deleted successfully", id);
    }
}
