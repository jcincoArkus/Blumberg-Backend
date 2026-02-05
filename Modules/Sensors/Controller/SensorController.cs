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
    /// <summary>
    /// Gets all sensors
    /// </summary>
    /// <returns>List of sensors</returns>
    [HttpGet(Name = "GetAllSensorsV1")]
    [ProducesResponseType(typeof(List<SensorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SensorResponse>>> GetAll()
    {
        logger.LogDebug("Getting all sensors");

        try
        {
            var sensors = await sensorService.GetAllAsync();
            logger.LogInformation("Retrieved {Count} sensors", sensors.Count);
            return Ok(sensors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all sensors");
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
    [HttpGet("{id}/readings", Name = "GetSensorReadingsV1")]
    [ProducesResponseType(typeof(PagedResponse<SensorReadingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<SensorReadingResponse>>> GetReadings(
        Guid id,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        logger.LogDebug("Getting readings for sensor {SensorId}, page {Page}, pageSize {PageSize}", id, page, pageSize);

        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var result = await sensorService.GetReadingsAsync(id, from, to, page, pageSize);
            logger.LogInformation("Retrieved {Count} readings for sensor {SensorId}", result.Items.Count, id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Sensor not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting readings for sensor {SensorId}", id);
            return StatusCode(500, new { message = "An error occurred while getting sensor readings" });
        }
    }

    /// <summary>
    /// Gets a sensor by ID
    /// </summary>
    /// <param name="id">Sensor ID</param>
    /// <returns>Sensor</returns>
    [HttpGet("{id}", Name = "GetSensorByIdV1")]
    [ProducesResponseType(typeof(SensorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SensorResponse>> GetById(Guid id)
    {
        logger.LogDebug("Getting sensor by ID: {Id}", id);

        try
        {
            var sensor = await sensorService.GetByIdAsync(id);
            logger.LogInformation("Retrieved sensor {Id}", id);
            return Ok(sensor);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Sensor not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sensor {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting sensor by ID" });
        }
    }

    /// <summary>
    /// Creates a new sensor
    /// </summary>
    /// <param name="request">Sensor information</param>
    /// <returns>Created sensor</returns>
    [HttpPost(Name = "CreateSensorV1")]
    [ProducesResponseType(typeof(SensorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SensorResponse>> Create([FromBody] SensorRequest request)
    {
        logger.LogDebug("Creating new sensor: {Serial}", request.Serial);

        try
        {
            var newSensor = await sensorService.CreateAsync(request);
            logger.LogInformation("Sensor created successfully with ID: {Id}", newSensor.Id);
            return CreatedAtAction(nameof(GetById), new { id = newSensor.Id }, newSensor);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Failed to create sensor: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating sensor: {Serial}", request.Serial);
            return StatusCode(500, new { message = "An error occurred while creating sensor" });
        }
    }

    /// <summary>
    /// Updates a sensor
    /// </summary>
    /// <param name="id">Sensor ID</param>
    /// <param name="request">Sensor information</param>
    /// <returns>Updated sensor</returns>
    [HttpPut("{id}", Name = "UpdateSensorV1")]
    [ProducesResponseType(typeof(SensorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SensorResponse>> Update(Guid id, [FromBody] SensorRequest request)
    {
        logger.LogDebug("Updating sensor {Id}", id);

        try
        {
            var updatedSensor = await sensorService.UpdateAsync(id, request);
            logger.LogInformation("Sensor {Id} updated successfully", id);
            return Ok(updatedSensor);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Sensor not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Failed to update sensor {Id}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating sensor {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating sensor" });
        }
    }

    /// <summary>
    /// Soft deletes a sensor
    /// </summary>
    /// <param name="id">Sensor ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}", Name = "DeleteSensorV1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        logger.LogDebug("Deleting sensor {Id}", id);

        try
        {
            await sensorService.DeleteAsync(id);
            logger.LogInformation("Sensor {Id} deleted successfully", id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Sensor not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting sensor {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting sensor" });
        }
    }
}
