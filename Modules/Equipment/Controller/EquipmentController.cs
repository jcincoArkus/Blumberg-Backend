using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Equipment.Dto;
using Modules.Equipment.Service;
using Shared.Dto;

namespace Modules.Equipment.Controller;

/// <summary>
/// Equipment controller for managing equipment operations
/// </summary>
[ApiController]
[Route("api/v1/equipment")]
[Tags("Equipment")]
[Authorize]
public class EquipmentController(IEquipmentService equipmentService, ILogger<EquipmentController> logger) : ControllerBase
{
    /// <summary>
    /// Gets paginated equipment
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated list of equipment</returns>
    [HttpGet(Name = "GetAllEquipmentV1")]
    [ProducesResponseType(typeof(PagedResponse<EquipmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<EquipmentResponse>>> GetAll([FromQuery] PaginationRequest request)
    {
        logger.LogDebug("Getting all equipment");

        try
        {
            var (items, totalCount) = await equipmentService.GetAllAsync(request);
            logger.LogInformation("Retrieved {Count} equipment", items.Count);
            return Ok(new PagedResponse<EquipmentResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all equipment");
            return StatusCode(500, new { message = "An error occurred while getting all equipment" });
        }
    }

    /// <summary>
    /// Gets equipment by ID
    /// </summary>
    /// <param name="id">Equipment ID</param>
    /// <returns>Equipment</returns>
    [HttpGet("{id}", Name = "GetEquipmentByIdV1")]
    [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipmentResponse>> GetById(Guid id)
    {
        logger.LogDebug("Getting equipment by ID: {Id}", id);

        try
        {
            var equipment = await equipmentService.GetByIdAsync(id);
            logger.LogInformation("Retrieved equipment {Id}", id);
            return Ok(MapToResponse(equipment));
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Equipment not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting equipment {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting equipment by ID" });
        }
    }

    /// <summary>
    /// Creates new equipment
    /// </summary>
    /// <param name="request">Equipment information</param>
    /// <returns>Created equipment</returns>
    [HttpPost(Name = "CreateEquipmentV1")]
    [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EquipmentResponse>> Create([FromBody] EquipmentRequest request)
    {
        logger.LogDebug("Creating new equipment: {Name}", request.Name);

        try
        {
            var newEquipment = await equipmentService.CreateAsync(request);
            var response = MapToResponse(newEquipment);
            logger.LogInformation("Equipment created successfully with ID: {Id}", response.Id);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Failed to create equipment: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating equipment: {Name}", request.Name);
            return StatusCode(500, new { message = "An error occurred while creating equipment" });
        }
    }

    /// <summary>
    /// Updates equipment
    /// </summary>
    /// <param name="id">Equipment ID</param>
    /// <param name="request">Equipment information</param>
    /// <returns>Updated equipment</returns>
    [HttpPut("{id}", Name = "UpdateEquipmentV1")]
    [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipmentResponse>> Update(Guid id, [FromBody] EquipmentRequest request)
    {
        logger.LogDebug("Updating equipment {Id}", id);

        try
        {
            var updatedEquipment = await equipmentService.UpdateAsync(id, request);
            logger.LogInformation("Equipment {Id} updated successfully", id);
            return Ok(MapToResponse(updatedEquipment));
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Equipment not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Failed to update equipment {Id}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating equipment {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating equipment" });
        }
    }

    /// <summary>
    /// Soft deletes equipment
    /// </summary>
    /// <param name="id">Equipment ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}", Name = "DeleteEquipmentV1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        logger.LogDebug("Deleting equipment {Id}", id);

        try
        {
            await equipmentService.DeleteAsync(id);
            logger.LogInformation("Equipment {Id} deleted successfully", id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Equipment not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting equipment {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting equipment" });
        }
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
