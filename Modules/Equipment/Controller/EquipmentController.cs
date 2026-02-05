using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Equipment.Service;
using Modules.Equipment.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

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
    /// Gets all equipment
    /// </summary>
    /// <returns>List of equipment</returns>
    [HttpGet(Name = "GetAllEquipmentV1")]
    [ProducesResponseType(typeof(List<EquipmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EquipmentResponse>>> GetAll()
    {
        logger.LogDebug("Getting all equipment");

        try
        {
            var equipment = await equipmentService.GetAllAsync();
            logger.LogInformation("Retrieved {Count} equipment", equipment.Count);
            return Ok(equipment);
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
            return Ok(equipment);
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
            logger.LogInformation("Equipment created successfully with ID: {Id}", newEquipment.Id);
            return CreatedAtAction(nameof(GetById), new { id = newEquipment.Id }, newEquipment);
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
            return Ok(updatedEquipment);
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
            return NoContent();
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
}
