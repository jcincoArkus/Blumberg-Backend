using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.SensorTypes.Dto;
using Modules.SensorTypes.Service;
using Shared.Dto;
using Shared.Entity;

namespace Modules.SensorTypes.Controller;

/// <summary>
/// Controller for sensor type CRUD (used when creating/editing sensors in the UI)
/// </summary>
[ApiController]
[Route("api/v1/sensor-types")]
[Tags("Sensor Types")]
[Authorize]
public class SensorTypesController(ISensorTypeService service, ILogger<SensorTypesController> logger) : ControllerBase
{
    /// <summary>Get paginated sensor types</summary>
    [HttpGet(Name = "GetAllSensorTypesV1")]
    [ProducesResponseType(typeof(PagedResponse<SensorTypeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<SensorTypeResponse>>> GetAll([FromQuery] PaginationRequest request)
    {
        try
        {
            var (items, totalCount) = await service.GetAllAsync(request);
            return Ok(new PagedResponse<SensorTypeResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sensor types");
            return StatusCode(500, new { message = "An error occurred while getting sensor types" });
        }
    }

    /// <summary>Get sensor type by ID</summary>
    [HttpGet("{id}", Name = "GetSensorTypeByIdV1")]
    [ProducesResponseType(typeof(SensorTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SensorTypeResponse>> GetById(Guid id)
    {
        try
        {
            var entity = await service.GetByIdAsync(id);
            return Ok(MapToResponse(entity));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Sensor type with ID {id} was not found" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sensor type {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting sensor type" });
        }
    }

    /// <summary>Create a sensor type</summary>
    [HttpPost(Name = "CreateSensorTypeV1")]
    [ProducesResponseType(typeof(SensorTypeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SensorTypeResponse>> Create([FromBody] SensorTypeRequest request)
    {
        try
        {
            var entity = await service.CreateAsync(request);
            var response = MapToResponse(entity);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating sensor type");
            return StatusCode(500, new { message = "An error occurred while creating sensor type" });
        }
    }

    /// <summary>Update a sensor type</summary>
    [HttpPut("{id}", Name = "UpdateSensorTypeV1")]
    [ProducesResponseType(typeof(SensorTypeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SensorTypeResponse>> Update(Guid id, [FromBody] SensorTypeRequest request)
    {
        try
        {
            var entity = await service.UpdateAsync(id, request);
            return Ok(MapToResponse(entity));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Sensor type with ID {id} was not found" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating sensor type {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating sensor type" });
        }
    }

    /// <summary>Soft delete a sensor type</summary>
    [HttpDelete("{id}", Name = "DeleteSensorTypeV1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await service.DeleteAsync(id);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Sensor type with ID {id} was not found" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting sensor type {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting sensor type" });
        }
    }

    private static SensorTypeResponse MapToResponse(SensorType entity)
    {
        return new SensorTypeResponse
        {
            Id = entity.Id,
            Type = entity.Type,
            Unit = entity.Unit,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
