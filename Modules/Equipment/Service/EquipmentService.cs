using Modules.Equipment.Dto;
using Modules.Equipment.Repository;
using Shared.Entity;

namespace Modules.Equipment.Service;

/// <summary>
/// Service implementation for equipment operations
/// </summary>
public class EquipmentService(IEquipmentRepository equipmentRepository) : IEquipmentService
{
    private readonly IEquipmentRepository _equipmentRepository = equipmentRepository;

    /// <inheritdoc />
    public async Task<List<EquipmentResponse>> GetAllAsync()
    {
        var equipment = await _equipmentRepository.GetAllAsync();
        return equipment.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<EquipmentResponse> GetByIdAsync(Guid id)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Equipment with ID {id} was not found");

        return MapToResponse(equipment);
    }

    /// <inheritdoc />
    public async Task<EquipmentResponse> CreateAsync(EquipmentRequest request)
    {
        var equipment = new Shared.Entity.Equipment
        {
            Name = request.Name,
            EquipmentType = request.EquipmentType,
            SiteId = request.SiteId
        };

        var createdEquipment = await _equipmentRepository.CreateAsync(equipment);
        return MapToResponse(createdEquipment);
    }

    /// <inheritdoc />
    public async Task<EquipmentResponse> UpdateAsync(Guid id, EquipmentRequest request)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Equipment with ID {id} was not found");

        equipment.Name = request.Name;
        equipment.EquipmentType = request.EquipmentType;
        equipment.SiteId = request.SiteId;

        var updatedEquipment = await _equipmentRepository.UpdateAsync(equipment);
        return MapToResponse(updatedEquipment);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        var deleted = await _equipmentRepository.SoftDeleteAsync(id);
        if (!deleted)
            throw new KeyNotFoundException($"Equipment with ID {id} was not found");
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
