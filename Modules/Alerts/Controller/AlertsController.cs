using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Alerts.Dto;
using Modules.Alerts.Repository;
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
public class AlertsController(IAlertService service, IRecommendedActionRepository recommendedActionRepository, ILogger<AlertsController> logger) : ControllerBase
{
    /// <summary>Get paginated alerts</summary>
    [HttpGet(Name = "GetAllAlertsV1")]
    [ProducesResponseType(typeof(PagedResponse<AlertResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<AlertResponse>>> GetAll([FromQuery] GetAlertsRequest request)
    {
        logger.LogDebug("Getting alerts page {Page}, size {PageSize}", request.Page, request.PageSize);
        try
        {
            var (items, totalCount) = await service.GetAllAsync(request);
            logger.LogInformation("Retrieved {Count} alerts (total {Total})", items.Count, totalCount);
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

    /// <summary>Get active (unresolved) alerts ordered by severity for the dashboard</summary>
    [HttpGet("active", Name = "GetActiveAlertsV1")]
    [ProducesResponseType(typeof(IReadOnlyList<ActiveAlertResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ActiveAlertResponse>>> GetActive()
    {
        logger.LogDebug("Getting active alerts for dashboard");
        try
        {
            var items = await service.GetActiveAsync();
            logger.LogInformation("Retrieved {Count} active alerts", items.Count);
            return Ok(items);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting active alerts");
            return StatusCode(500, new { message = "An error occurred while getting active alerts" });
        }
    }

    /// <summary>Get alert by ID</summary>
    [HttpGet("{id}", Name = "GetAlertByIdV1")]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertResponse>> GetById(Guid id)
    {
        logger.LogDebug("Getting alert by ID: {Id}", id);
        try
        {
            var entity = await service.GetByIdAsync(id);
            logger.LogInformation("Retrieved alert {Id}", id);
            var recommendedActions = entity.Sensor != null
                ? await recommendedActionRepository.GetActiveBySensorTypeAndSeverityAsync(entity.Sensor.SensorTypeId, entity.Severity)
                : [];
            return Ok(MapToResponse(entity, recommendedActions));
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("Alert not found with ID: {Id}", id);
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
        logger.LogDebug("Acknowledging alert {Id}", id);
        try
        {
            var entity = await service.AcknowledgeAsync(id);
            logger.LogInformation("Alert {Id} acknowledged", id);
            return Ok(MapToResponse(entity));
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("Alert not found with ID: {Id}", id);
            return NotFound(new { message = $"Alert with ID {id} was not found" });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Failed to acknowledge alert {Id}: {Message}", id, ex.Message);
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
        logger.LogDebug("Resolving alert {Id}", id);
        try
        {
            var entity = await service.ResolveAsync(id);
            logger.LogInformation("Alert {Id} resolved", id);
            return Ok(MapToResponse(entity));
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("Alert not found with ID: {Id}", id);
            return NotFound(new { message = $"Alert with ID {id} was not found" });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Failed to resolve alert {Id}: {Message}", id, ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resolving alert {Id}", id);
            return StatusCode(500, new { message = "An error occurred while resolving alert" });
        }
    }

    private static AlertResponse MapToResponse(Alert entity, IReadOnlyList<Shared.Entity.RecommendedAction>? recommendedActions = null)
    {
        var response = new AlertResponse
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
        if (entity.Events?.Count > 0)
        {
            response.Events = entity.Events
                .OrderBy(e => e.OccurredAt)
                .Select(e => new AlertEventResponse
                {
                    Id = e.Id,
                    EventType = e.EventType.ToString(),
                    OccurredAt = e.OccurredAt,
                    Description = e.Description,
                    ActorId = e.ActorId,
                })
                .ToList();
        }
        // When recommendedActions is provided (e.g. GetById), always set the list (empty if none configured)
        if (recommendedActions != null)
        {
            response.RecommendedActions = recommendedActions
                .Select(a => new RecommendedActionResponse
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    DisplayOrder = a.DisplayOrder,
                })
                .ToList();
        }
        return response;
    }
}
