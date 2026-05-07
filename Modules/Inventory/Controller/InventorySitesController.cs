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
[Route("api/v1/inventory/sites")]
[Tags("Inventory - Sites")]
[Authorize]
public class InventorySitesController(IInventorySiteService service, ILogger<InventorySitesController> logger) : ControllerBase
{
    [HttpGet(Name = "GetAllInventorySitesV1")]
    [ProducesResponseType(typeof(PagedResponse<InventorySiteResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<InventorySiteResponse>>> GetAll([FromQuery] PaginationRequest request)
    {
        try
        {
            var (items, totalCount) = await service.GetAllAsync(request);
            return Ok(new PagedResponse<InventorySiteResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting inventory sites");
            return StatusCode(500, new { message = "An error occurred while getting inventory sites" });
        }
    }

    [HttpGet("{id}", Name = "GetInventorySiteByIdV1")]
    [ProducesResponseType(typeof(InventorySiteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventorySiteResponse>> GetById(Guid id)
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
            logger.LogError(ex, "Error getting inventory site {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting inventory site" });
        }
    }

    [HttpPost(Name = "CreateInventorySiteV1")]
    [ProducesResponseType(typeof(InventorySiteResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<InventorySiteResponse>> Create([FromBody] InventorySiteRequest request)
    {
        try
        {
            var entity = await service.CreateAsync(request);
            var response = MapToResponse(entity);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating inventory site");
            return StatusCode(500, new { message = "An error occurred while creating inventory site" });
        }
    }

    [HttpPut("{id}", Name = "UpdateInventorySiteV1")]
    [ProducesResponseType(typeof(InventorySiteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventorySiteResponse>> Update(Guid id, [FromBody] InventorySiteRequest request)
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
            logger.LogError(ex, "Error updating inventory site {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating inventory site" });
        }
    }

    [HttpDelete("{id}", Name = "DeleteInventorySiteV1")]
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
            logger.LogError(ex, "Error deleting inventory site {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting inventory site" });
        }
    }

    private static InventorySiteResponse MapToResponse(InventorySite e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt ?? e.CreatedAt
    };
}
