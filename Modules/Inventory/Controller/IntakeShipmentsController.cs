using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Modules.Inventory.Service;
using Shared.Entity;

namespace Modules.Inventory.Controller;

[ApiController]
[Route("api/v1/inventory/intake-shipments")]
[Tags("Inventory - Intake Shipments")]
[Authorize]
public class IntakeShipmentsController(IIntakeShipmentService service, ILogger<IntakeShipmentsController> logger) : ControllerBase
{
    [HttpGet(Name = "GetAllIntakeShipmentsV1")]
    [ProducesResponseType(typeof(PagedResponse<IntakeShipmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<IntakeShipmentResponse>>> GetAll([FromQuery] GetIntakeShipmentsRequest request)
    {
        try
        {
            var (items, totalCount) = await service.GetAllAsync(request);
            return Ok(new PagedResponse<IntakeShipmentResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting intake shipments");
            return StatusCode(500, new { message = "An error occurred while getting intake shipments" });
        }
    }

    [HttpGet("{id}", Name = "GetIntakeShipmentByIdV1")]
    [ProducesResponseType(typeof(IntakeShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IntakeShipmentResponse>> GetById(Guid id)
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
            logger.LogError(ex, "Error getting intake shipment {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting intake shipment" });
        }
    }

    [HttpPost(Name = "CreateIntakeShipmentV1")]
    [ProducesResponseType(typeof(IntakeShipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IntakeShipmentResponse>> Create([FromBody] IntakeShipmentRequest request)
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
            logger.LogError(ex, "Error creating intake shipment");
            return StatusCode(500, new { message = "An error occurred while creating intake shipment" });
        }
    }

    [HttpPut("{id}", Name = "UpdateIntakeShipmentV1")]
    [ProducesResponseType(typeof(IntakeShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IntakeShipmentResponse>> Update(Guid id, [FromBody] IntakeShipmentRequest request)
    {
        try
        {
            return Ok(MapToResponse(await service.UpdateAsync(id, request)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating intake shipment {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating intake shipment" });
        }
    }

    [HttpDelete("{id}", Name = "DeleteIntakeShipmentV1")]
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
            logger.LogError(ex, "Error deleting intake shipment {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting intake shipment" });
        }
    }

    private static IntakeShipmentResponse MapToResponse(IntakeShipment e) => new()
    {
        Id = e.Id,
        PoReference = e.PoReference,
        SupplierId = e.SupplierId,
        SupplierName = e.Supplier?.Name ?? string.Empty,
        Vehicle = e.Vehicle,
        Driver = e.Driver,
        SiteId = e.SiteId,
        SiteName = e.Site?.Name ?? string.Empty,
        ReceivingZone = e.ReceivingZone,
        ColdChainTempC = e.ColdChainTempC,
        ArrivedAt = e.ArrivedAt,
        ReceivedBy = e.ReceivedBy,
        Status = e.Status,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt ?? e.CreatedAt
    };
}
