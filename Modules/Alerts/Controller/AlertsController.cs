using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Alerts.Dto;
using Modules.Alerts.Service;
using Shared.Entity;

namespace Modules.Alerts.Controller;

/// <summary>
/// Controller for alert management
/// </summary>
[ApiController]
[Route("api/v1/alerts")]
[Tags("Alerts")]
[Authorize]
public class AlertsController(IAlertService service, ILogger<AlertsController> logger) : ControllerBase
{
    /// <summary>Get paginated alerts</summary>
    [HttpGet(Name = "GetAllAlertsV1")]
    [ProducesResponseType(typeof(PagedResponse<AlertResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<AlertResponse>>> GetAll([FromQuery] GetAlertsRequest request)
    {
        try
        {
            var (items, totalCount) = await service.GetAllAsync(request);
            return Ok(new PagedResponse<AlertResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting alerts");
            return StatusCode(500, new { message = "An error occurred while getting alerts" });
        }
    }

    /// <summary>Get alert by ID</summary>
    [HttpGet("{id}", Name = "GetAlertByIdV1")]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertResponse>> GetById(Guid id)
    {
        try
        {
            var entity = await service.GetByIdAsync(id);
            return Ok(MapToResponse(entity));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Alert with ID {id} was not found" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting alert {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting alert" });
        }
    }

    /// <summary>Acknowledge an active alert</summary>
    [HttpPatch("{id}/acknowledge", Name = "AcknowledgeAlertV1")]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AlertResponse>> Acknowledge(Guid id)
    {
        try
        {
            var entity = await service.AcknowledgeAsync(id);
            return Ok(MapToResponse(entity));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Alert with ID {id} was not found" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error acknowledging alert {Id}", id);
            return StatusCode(500, new { message = "An error occurred while acknowledging alert" });
        }
    }

    /// <summary>Resolve an active or acknowledged alert</summary>
    [HttpPatch("{id}/resolve", Name = "ResolveAlertV1")]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AlertResponse>> Resolve(Guid id)
    {
        try
        {
            var entity = await service.ResolveAsync(id);
            return Ok(MapToResponse(entity));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Alert with ID {id} was not found" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resolving alert {Id}", id);
            return StatusCode(500, new { message = "An error occurred while resolving alert" });
        }
    }

    private static AlertResponse MapToResponse(Alert entity) => new()
    {
        Id = entity.Id,
        SensorId = entity.SensorId,
        EquipmentId = entity.EquipmentId,
        SiteId = entity.SiteId,
        Severity = entity.Severity.ToString(),
        TriggeredValue = entity.TriggeredValue,
        ThresholdMin = entity.ThresholdMin,
        ThresholdMax = entity.ThresholdMax,
        TriggeredAt = entity.TriggeredAt,
        Status = entity.Status.ToString(),
        ResolvedAt = entity.ResolvedAt,
        CreatedAt = entity.CreatedAt,
        EquipmentName = entity.Equipment?.Name,
        SensorSerial = entity.Sensor?.Serial,
        SensorTypeName = entity.Sensor?.SensorType?.Type.ToString(),
    };
}
