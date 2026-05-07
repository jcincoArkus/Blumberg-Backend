using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Modules.Inventory.Service;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Controller;

[ApiController]
[Route("api/v1/inventory/site-zones")]
[Tags("Inventory - Site Zones")]
[Authorize]
public class SiteZonesController(ISiteZoneService service, ILogger<SiteZonesController> logger) : ControllerBase
{
    [HttpGet(Name = "GetAllSiteZonesV1")]
    [ProducesResponseType(typeof(PagedResponse<SiteZoneResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<SiteZoneResponse>>> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? siteId)
    {
        try
        {
            var (items, totalCount) = await service.GetAllAsync(request, siteId);
            return Ok(new PagedResponse<SiteZoneResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting site zones");
            return StatusCode(500, new { message = "An error occurred while getting site zones" });
        }
    }

    [HttpGet("{id}", Name = "GetSiteZoneByIdV1")]
    [ProducesResponseType(typeof(SiteZoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteZoneResponse>> GetById(Guid id)
    {
        try
        {
            return Ok(MapToResponse(await service.GetByIdAsync(id)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting site zone {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting site zone" });
        }
    }

    [HttpPost(Name = "CreateSiteZoneV1")]
    [ProducesResponseType(typeof(SiteZoneResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SiteZoneResponse>> Create([FromBody] SiteZoneRequest request)
    {
        try
        {
            var entity = await service.CreateAsync(request);
            var response = MapToResponse(entity);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating site zone");
            return StatusCode(500, new { message = "An error occurred while creating site zone" });
        }
    }

    [HttpPut("{id}", Name = "UpdateSiteZoneV1")]
    [ProducesResponseType(typeof(SiteZoneResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteZoneResponse>> Update(Guid id, [FromBody] SiteZoneRequest request)
    {
        try
        {
            return Ok(MapToResponse(await service.UpdateAsync(id, request)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating site zone {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating site zone" });
        }
    }

    [HttpDelete("{id}", Name = "DeleteSiteZoneV1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await service.DeleteAsync(id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting site zone {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting site zone" });
        }
    }

    private static SiteZoneResponse MapToResponse(SiteZone e) => new()
    {
        Id = e.Id,
        SiteId = e.SiteId,
        SiteName = e.Site?.Name ?? string.Empty,
        Name = e.Name,
        CreatedAt = e.CreatedAt
    };
}
