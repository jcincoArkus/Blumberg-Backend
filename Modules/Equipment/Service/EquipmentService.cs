using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Equipment.Dto;
using Modules.Equipment.Repository;
using Shared.Entity;

namespace Modules.Equipment.Service;

/// <summary>
/// Service implementation for equipment operations
/// </summary>
public class EquipmentService(IEquipmentRepository equipmentRepository, ILogger<EquipmentService> logger) : IEquipmentService
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<List<EquipmentResponse>> GetAllAsync()
    {
        logger.LogDebug("Getting all equipment from repository");

        var equipment = await equipmentRepository.GetAllAsync();

        logger.LogInformation("Retrieved {Count} equipment", equipment.Count);

        return equipment.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<EquipmentResponse> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Getting equipment by ID: {Id}", id);

        var equipment = await equipmentRepository.GetByIdAsync(id);

        if (equipment == null)
        {
            logger.LogWarning("Equipment not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Equipment with ID {id} was not found");
        }

        logger.LogInformation("Retrieved equipment {Id}", id);

        return MapToResponse(equipment);
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<EquipmentResponse> CreateAsync(EquipmentRequest request)
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

        return MapToResponse(createdEquipment);
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<EquipmentResponse> UpdateAsync(Guid id, EquipmentRequest request)
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

        return MapToResponse(updatedEquipment);
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

    /// <summary>
    /// Maps an Equipment entity to an EquipmentResponse DTO
    /// </summary>
    private static EquipmentResponse MapToResponse(Shared.Entity.Equipment equipment)
    {
        return new EquipmentResponse
        {
            Id = equipment.Id,
            Name = equipment.Name,
            EquipmentType = equipment.EquipmentType,
            OrganizationId = equipment.OrganizationId,
            OrganizationName = equipment.Organization?.Name ?? string.Empty,
            SiteId = equipment.SiteId,
            SiteName = equipment.Site?.Name ?? string.Empty,
            CreatedAt = equipment.CreatedAt,
            UpdatedAt = equipment.UpdatedAt
        };
    }
}
