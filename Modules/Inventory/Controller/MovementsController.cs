using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Modules.Inventory.Service;
using Shared.Entity;

namespace Modules.Inventory.Controller;

[ApiController]
[Route("api/v1/inventory/movements")]
[Tags("Inventory - Movements")]
[Authorize]
public class MovementsController(IMovementService service, ILogger<MovementsController> logger) : ControllerBase
{
    [HttpGet(Name = "GetAllMovementsV1")]
    [ProducesResponseType(typeof(PagedResponse<MovementResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<MovementResponse>>> GetAll([FromQuery] GetMovementsRequest request)
    {
        try
        {
            var (items, totalCount) = await service.GetAllAsync(request);
            return Ok(new PagedResponse<MovementResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting movements");
            return StatusCode(500, new { message = "An error occurred while getting movements" });
        }
    }

    [HttpGet("{id}", Name = "GetMovementByIdV1")]
    [ProducesResponseType(typeof(MovementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovementResponse>> GetById(Guid id)
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
            logger.LogError(ex, "Error getting movement {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting movement" });
        }
    }

    [HttpPost(Name = "CreateMovementV1")]
    [ProducesResponseType(typeof(MovementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovementResponse>> Create([FromBody] MovementRequest request)
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
            logger.LogError(ex, "Error creating movement");
            return StatusCode(500, new { message = "An error occurred while creating movement" });
        }
    }

    private static MovementResponse MapToResponse(Movement e) => new()
    {
        Id = e.Id,
        Type = e.Type,
        OccurredAt = e.OccurredAt,
        ProductId = e.ProductId,
        ProductName = e.Product?.Name ?? string.Empty,
        ProductSku = e.Product?.Sku ?? string.Empty,
        Qty = e.Qty,
        Unit = e.Unit,
        LotCode = e.LotCode,
        SiteId = e.SiteId,
        SiteName = e.Site?.Name ?? string.Empty,
        DestSiteId = e.DestSiteId,
        DestSiteName = e.DestSite?.Name,
        PerformedBy = e.PerformedBy,
        Note = e.Note,
        CreatedAt = e.CreatedAt
    };
}
