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
[Route("api/v1/inventory/shipment-lines")]
[Tags("Inventory - Shipment Lines")]
[Authorize]
public class ShipmentLinesController(IShipmentLineService service, ILogger<ShipmentLinesController> logger) : ControllerBase
{
    [HttpGet(Name = "GetAllShipmentLinesV1")]
    [ProducesResponseType(typeof(PagedResponse<ShipmentLineResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ShipmentLineResponse>>> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? shipmentId)
    {
        try
        {
            var (items, totalCount) = await service.GetAllAsync(request, shipmentId);
            return Ok(new PagedResponse<ShipmentLineResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting shipment lines");
            return StatusCode(500, new { message = "An error occurred while getting shipment lines" });
        }
    }

    [HttpGet("{id}", Name = "GetShipmentLineByIdV1")]
    [ProducesResponseType(typeof(ShipmentLineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShipmentLineResponse>> GetById(Guid id)
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
            logger.LogError(ex, "Error getting shipment line {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting shipment line" });
        }
    }

    [HttpPost(Name = "CreateShipmentLineV1")]
    [ProducesResponseType(typeof(ShipmentLineResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ShipmentLineResponse>> Create([FromBody] ShipmentLineRequest request)
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
            logger.LogError(ex, "Error creating shipment line");
            return StatusCode(500, new { message = "An error occurred while creating shipment line" });
        }
    }

    [HttpPut("{id}", Name = "UpdateShipmentLineV1")]
    [ProducesResponseType(typeof(ShipmentLineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShipmentLineResponse>> Update(Guid id, [FromBody] ShipmentLineRequest request)
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
            logger.LogError(ex, "Error updating shipment line {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating shipment line" });
        }
    }

    [HttpDelete("{id}", Name = "DeleteShipmentLineV1")]
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
            logger.LogError(ex, "Error deleting shipment line {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting shipment line" });
        }
    }

    private static ShipmentLineResponse MapToResponse(IntakeShipmentLine e) => new()
    {
        Id = e.Id,
        ShipmentId = e.ShipmentId,
        PoReference = e.Shipment?.PoReference ?? string.Empty,
        ProductId = e.ProductId,
        ProductName = e.Product?.Name ?? string.Empty,
        ProductSku = e.Product?.Sku ?? string.Empty,
        LotCode = e.LotCode,
        Qty = e.Qty,
        Unit = e.Unit,
        CostPerUnit = e.CostPerUnit,
        CreatedAt = e.CreatedAt
    };
}
