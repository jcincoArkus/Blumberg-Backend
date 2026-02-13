using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Thresholds.Dto;
using Modules.Thresholds.Service;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Thresholds.Controller;

/// <summary>
/// Controller for threshold CRUD (used when creating/editing sensors in the UI)
/// </summary>
[ApiController]
[Route("api/v1/thresholds")]
[Tags("Thresholds")]
[Authorize]
public class ThresholdsController(IThresholdService service, ILogger<ThresholdsController> logger) : ControllerBase
{
    /// <summary>Get paginated thresholds</summary>
    [HttpGet(Name = "GetAllThresholdsV1")]
    [ProducesResponseType(typeof(PagedResponse<ThresholdResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ThresholdResponse>>> GetAll([FromQuery] PaginationRequest request)
    {
        try
        {
            var (items, totalCount) = await service.GetAllAsync(request);
            return Ok(new PagedResponse<ThresholdResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting thresholds");
            return StatusCode(500, new { message = "An error occurred while getting thresholds" });
        }
    }

    /// <summary>Get threshold by ID</summary>
    [HttpGet("{id}", Name = "GetThresholdByIdV1")]
    [ProducesResponseType(typeof(ThresholdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ThresholdResponse>> GetById(Guid id)
    {
        try
        {
            var entity = await service.GetByIdAsync(id);
            return Ok(MapToResponse(entity));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Threshold with ID {id} was not found" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting threshold {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting threshold" });
        }
    }

    /// <summary>Create a threshold</summary>
    [HttpPost(Name = "CreateThresholdV1")]
    [ProducesResponseType(typeof(ThresholdResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ThresholdResponse>> Create([FromBody] ThresholdRequest request)
    {
        try
        {
            var entity = await service.CreateAsync(request);
            var response = MapToResponse(entity);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating threshold");
            return StatusCode(500, new { message = "An error occurred while creating threshold" });
        }
    }

    /// <summary>Update a threshold</summary>
    [HttpPut("{id}", Name = "UpdateThresholdV1")]
    [ProducesResponseType(typeof(ThresholdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ThresholdResponse>> Update(Guid id, [FromBody] ThresholdRequest request)
    {
        try
        {
            var entity = await service.UpdateAsync(id, request);
            return Ok(MapToResponse(entity));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Threshold with ID {id} was not found" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating threshold {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating threshold" });
        }
    }

    /// <summary>Soft delete a threshold</summary>
    [HttpDelete("{id}", Name = "DeleteThresholdV1")]
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
            return NotFound(new { message = $"Threshold with ID {id} was not found" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting threshold {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting threshold" });
        }
    }

    private static ThresholdResponse MapToResponse(Threshold entity)
    {
        return new ThresholdResponse
        {
            Id = entity.Id,
            Min = entity.Min,
            Max = entity.Max,
            DurationSeconds = (int)entity.Duration.TotalSeconds,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
