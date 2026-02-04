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
    private readonly IEquipmentService _equipmentService = equipmentService;
    private readonly ILogger<EquipmentController> _logger = logger;

    /// <summary>
    /// Gets all equipment
    /// </summary>
    /// <returns>List of equipment</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<EquipmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EquipmentResponse>>> GetAll()
    {
        try
        {
            var equipment = await _equipmentService.GetAllAsync();
            return Ok(equipment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all equipment");
            return StatusCode(500, new { message = "An error occurred while getting all equipment" });
        }
    }

    /// <summary>
    /// Gets equipment by ID
    /// </summary>
    /// <param name="id">Equipment ID</param>
    /// <returns>Equipment</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipmentResponse>> GetById(Guid id)
    {
        try
        {
            var equipment = await _equipmentService.GetByIdAsync(id);
            return Ok(equipment);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Equipment not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting equipment by ID");
            return StatusCode(500, new { message = "An error occurred while getting equipment by ID" });
        }
    }

    /// <summary>
    /// Creates new equipment
    /// </summary>
    /// <param name="request">Equipment information</param>
    /// <returns>Created equipment</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EquipmentResponse>> Create([FromBody] EquipmentRequest request)
    {
        try
        {
            var newEquipment = await _equipmentService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = newEquipment.Id }, newEquipment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create equipment: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating equipment");
            return StatusCode(500, new { message = "An error occurred while creating equipment" });
        }
    }

    /// <summary>
    /// Updates equipment
    /// </summary>
    /// <param name="id">Equipment ID</param>
    /// <param name="request">Equipment information</param>
    /// <returns>Updated equipment</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EquipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipmentResponse>> Update(Guid id, [FromBody] EquipmentRequest request)
    {
        try
        {
            var updatedEquipment = await _equipmentService.UpdateAsync(id, request);
            return Ok(updatedEquipment);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Equipment not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update equipment: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating equipment");
            return StatusCode(500, new { message = "An error occurred while updating equipment" });
        }
    }

    /// <summary>
    /// Soft deletes equipment
    /// </summary>
    /// <param name="id">Equipment ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await _equipmentService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Equipment not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting equipment");
            return StatusCode(500, new { message = "An error occurred while deleting equipment" });
        }
    }
}
