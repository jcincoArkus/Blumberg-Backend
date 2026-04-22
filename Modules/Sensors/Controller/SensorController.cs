using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Sensors.Dto;
using Modules.Sensors.Service;
using Shared.Dto;

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
    /// Gets paginated sensor health list with optional filters (siteId, equipmentId, status, healthStatus).
    /// </summary>
    /// <param name="request">Filters and pagination</param>
    /// <returns>Paginated list of sensors with health status, lastSeenAt, reliability</returns>
    [HttpGet("health", Name = "GetSensorHealthListV1")]
    [ProducesResponseType(typeof(PagedResponse<SensorHealthListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<SensorHealthListItemResponse>>> GetHealthList([FromQuery] GetSensorHealthRequest request)
    {
        logger.LogDebug("Getting sensor health list, page {Page}, pageSize {PageSize}", request.Page, request.PageSize);

        try
        {
            var (items, totalCount) = await sensorService.GetHealthListAsync(request);
            return Ok(new PagedResponse<SensorHealthListItemResponse>
            {
                Items = items.Select(MapHealthListToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sensor health list");
            return StatusCode(500, new { message = "An error occurred while getting sensor health list" });
        }
    }

    /// <summary>
    /// Gets detailed health for a single sensor (current value, freshness, reliability, recent readings, expected vs actual).
    /// </summary>
    /// <param name="id">Sensor ID</param>
    /// <returns>Health detail</returns>
    [HttpGet("{id}/health", Name = "GetSensorHealthByIdV1")]
    [ProducesResponseType(typeof(SensorHealthDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SensorHealthDetailResponse>> GetHealthById(Guid id)
    {
        logger.LogDebug("Getting sensor health for {Id}", id);

        try
        {
            var result = await sensorService.GetHealthDetailAsync(id);
            return Ok(MapHealthDetailToResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Sensor not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sensor health for {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting sensor health" });
        }
    }

    /// <summary>
    /// Gets paginated sensors
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated list of sensors</returns>
    [HttpGet(Name = "GetAllSensorsV1")]
    [ProducesResponseType(typeof(PagedResponse<SensorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<SensorResponse>>> GetAll([FromQuery] PaginationRequest request)
    {
        logger.LogDebug("Getting all sensors");

        try
        {
            var (items, totalCount) = await sensorService.GetAllAsync(request);
            logger.LogInformation("Retrieved {Count} sensors", items.Count);
            return Ok(new PagedResponse<SensorResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
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
    /// <param name="request">Query parameters (pagination + time range)</param>
    /// <returns>Paginated readings</returns>
    [HttpGet("{id}/readings", Name = "GetSensorReadingsV1")]
    [ProducesResponseType(typeof(PagedResponse<SensorReadingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<SensorReadingResponse>>> GetReadings(
        Guid id,
        [FromQuery] GetSensorReadingsRequest request)
    {
        logger.LogDebug("Getting readings for sensor {SensorId}, page {Page}, pageSize {PageSize}", id, request.Page, request.PageSize);

        try
        {
            var (items, totalCount) = await sensorService.GetReadingsAsync(id, request);
            logger.LogInformation("Retrieved {Count} readings for sensor {SensorId}", items.Count, id);
            return Ok(new PagedResponse<SensorReadingResponse>
            {
                Items = items.Select(MapReadingToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
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
            return Ok(MapToResponse(sensor));
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
            var response = MapToResponse(newSensor);
            logger.LogInformation("Sensor created successfully with ID: {Id}", response.Id);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
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
            return Ok(MapToResponse(updatedSensor));
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

    /// <summary>
    /// Maps a Sensor entity to a SensorResponse DTO
    /// </summary>
    private static SensorResponse MapToResponse(Shared.Entity.Sensor sensor)
    {
        return new SensorResponse
        {
            Id = sensor.Id,
            Serial = sensor.Serial,
            Status = sensor.Status,
            OrganizationId = sensor.OrganizationId,
            OrganizationName = sensor.Organization?.Name ?? string.Empty,
            EquipmentId = sensor.EquipmentId,
            EquipmentName = sensor.Equipment?.Name ?? string.Empty,
            SensorTypeId = sensor.SensorTypeId,
            SensorTypeName = sensor.SensorType?.Type.ToString() ?? string.Empty,
            ThresholdId = sensor.ThresholdId,
            CreatedAt = sensor.CreatedAt,
            UpdatedAt = sensor.UpdatedAt
        };
    }

    /// <summary>
    /// Maps a SensorReading entity to a SensorReadingResponse DTO
    /// </summary>
    private static SensorReadingResponse MapReadingToResponse(Shared.Entity.SensorReading reading)
    {
        return new SensorReadingResponse
        {
            Id = reading.Id,
            SensorId = reading.SensorId,
            Value = reading.Value,
            TimestampUtc = reading.TimestampUtc,
            Unit = reading.Unit,
            OrganizationId = reading.OrganizationId,
            IngestionRunId = reading.IngestionRunId,
            CreatedAt = reading.CreatedAt
        };
    }

    private static SensorHealthListItemResponse MapHealthListToResponse(SensorHealthListResult result)
    {
        return new SensorHealthListItemResponse
        {
            Id = result.Id,
            Name = result.Name,
            SensorType = result.SensorType,
            HealthStatus = result.HealthStatus,
            LastSeenAt = result.LastSeenAt,
            ReliabilityScore = result.ReliabilityScore,
            SiteId = result.SiteId,
            SiteName = result.SiteName,
            EquipmentId = result.EquipmentId,
            EquipmentName = result.EquipmentName,
            LastValue = result.LastValue,
            Unit = result.Unit,
            IngestionSource = result.IngestionSource
        };
    }

    private static SensorHealthDetailResponse MapHealthDetailToResponse(SensorHealthDetailResult result)
    {
        return new SensorHealthDetailResponse
        {
            SensorId = result.SensorId,
            Name = result.Name,
            HealthStatus = result.HealthStatus,
            LastSeenAt = result.LastSeenAt,
            ReliabilityScore = result.ReliabilityScore,
            LastValue = result.LastValue,
            Unit = result.Unit,
            FreshnessSeconds = result.FreshnessSeconds,
            RecentReadingsCount = result.RecentReadingsCount,
            ExpectedPoints = result.ExpectedPoints,
            ReceivedPoints = result.ReceivedPoints
        };
    }
}
