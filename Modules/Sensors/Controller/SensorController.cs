using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Sensors.Service;
using Modules.Sensors.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Modules.Sensors.Controller;

/// <summary>
/// Sensor controller for managing sensor operations
/// </summary>
[ApiController]
[Route("api/v1/sensors")]
[Tags("Sensors")]
[Authorize]
public class SensorController(ISensorService sensorService, ILogger<SensorController> logger) : ControllerBase
{
    private readonly ISensorService _sensorService = sensorService;
    private readonly ILogger<SensorController> _logger = logger;

    /// <summary>
    /// Gets all sensors
    /// </summary>
    /// <returns>List of sensors</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<SensorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SensorResponse>>> GetAll()
    {
        try
        {
            var sensors = await _sensorService.GetAllAsync();
            return Ok(sensors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all sensors");
            return StatusCode(500, new { message = "An error occurred while getting all sensors" });
        }
    }

    /// <summary>
    /// Gets paginated readings for a sensor
    /// </summary>
    /// <param name="id">Sensor ID</param>
    /// <param name="from">Optional start of time range (UTC, inclusive)</param>
    /// <param name="to">Optional end of time range (UTC, inclusive)</param>
    /// <param name="page">1-based page number (default 1)</param>
    /// <param name="pageSize">Page size (default 20)</param>
    /// <returns>Paginated readings</returns>
    [HttpGet("{id}/readings")]
    [ProducesResponseType(typeof(PagedResponse<SensorReadingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<SensorReadingResponse>>> GetReadings(
        Guid id,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var result = await _sensorService.GetReadingsAsync(id, from, to, page, pageSize);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Sensor not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting readings for sensor {SensorId}", id);
            return StatusCode(500, new { message = "An error occurred while getting sensor readings" });
        }
    }

    /// <summary>
    /// Gets a sensor by ID
    /// </summary>
    /// <param name="id">Sensor ID</param>
    /// <returns>Sensor</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SensorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SensorResponse>> GetById(Guid id)
    {
        try
        {
            var sensor = await _sensorService.GetByIdAsync(id);
            return Ok(sensor);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Sensor not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sensor by ID");
            return StatusCode(500, new { message = "An error occurred while getting sensor by ID" });
        }
    }

    /// <summary>
    /// Creates a new sensor
    /// </summary>
    /// <param name="request">Sensor information</param>
    /// <returns>Created sensor</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SensorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SensorResponse>> Create([FromBody] SensorRequest request)
    {
        try
        {
            var newSensor = await _sensorService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = newSensor.Id }, newSensor);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create sensor: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sensor");
            return StatusCode(500, new { message = "An error occurred while creating sensor" });
        }
    }

    /// <summary>
    /// Updates a sensor
    /// </summary>
    /// <param name="id">Sensor ID</param>
    /// <param name="request">Sensor information</param>
    /// <returns>Updated sensor</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SensorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SensorResponse>> Update(Guid id, [FromBody] SensorRequest request)
    {
        try
        {
            var updatedSensor = await _sensorService.UpdateAsync(id, request);
            return Ok(updatedSensor);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Sensor not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update sensor: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sensor");
            return StatusCode(500, new { message = "An error occurred while updating sensor" });
        }
    }

    /// <summary>
    /// Soft deletes a sensor
    /// </summary>
    /// <param name="id">Sensor ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await _sensorService.DeleteAsync(id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Sensor not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sensor");
            return StatusCode(500, new { message = "An error occurred while deleting sensor" });
        }
    }
}
