using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Modules.Inventory.Service;
using Shared.Entity;

namespace Modules.Inventory.Controller;

[ApiController]
[Route("api/v1/inventory/lots")]
[Tags("Inventory - Lots")]
[Authorize]
public class LotsController(ILotService service, ILogger<LotsController> logger) : ControllerBase
{
    [HttpGet(Name = "GetAllLotsV1")]
    [ProducesResponseType(typeof(PagedResponse<LotResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<LotResponse>>> GetAll([FromQuery] GetLotsRequest request)
    {
        try
        {
            var (items, totalCount) = await service.GetAllAsync(request);
            return Ok(new PagedResponse<LotResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting lots");
            return StatusCode(500, new { message = "An error occurred while getting lots" });
        }
    }

    [HttpGet("{lotCode}", Name = "GetLotByCodeV1")]
    [ProducesResponseType(typeof(LotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LotResponse>> GetByCode(string lotCode)
    {
        try
        {
            return Ok(MapToResponse(await service.GetByCodeAsync(lotCode)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting lot {LotCode}", lotCode);
            return StatusCode(500, new { message = "An error occurred while getting lot" });
        }
    }

    [HttpPost(Name = "CreateLotV1")]
    [ProducesResponseType(typeof(LotResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LotResponse>> Create([FromBody] LotRequest request)
    {
        try
        {
            var entity = await service.CreateAsync(request);
            var response = MapToResponse(entity);
            return CreatedAtAction(nameof(GetByCode), new { lotCode = response.LotCode }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating lot");
            return StatusCode(500, new { message = "An error occurred while creating lot" });
        }
    }

    [HttpPut("{lotCode}", Name = "UpdateLotV1")]
    [ProducesResponseType(typeof(LotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LotResponse>> Update(string lotCode, [FromBody] LotRequest request)
    {
        try
        {
            return Ok(MapToResponse(await service.UpdateAsync(lotCode, request)));
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
            logger.LogError(ex, "Error updating lot {LotCode}", lotCode);
            return StatusCode(500, new { message = "An error occurred while updating lot" });
        }
    }

    [HttpDelete("{lotCode}", Name = "DeleteLotV1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string lotCode)
    {
        try
        {
            await service.DeleteAsync(lotCode);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting lot {LotCode}", lotCode);
            return StatusCode(500, new { message = "An error occurred while deleting lot" });
        }
    }

    private static LotResponse MapToResponse(Lot e) => new()
    {
        LotCode = e.LotCode,
        ProductId = e.ProductId,
        ProductName = e.Product?.Name ?? string.Empty,
        ProductSku = e.Product?.Sku ?? string.Empty,
        Qty = e.Qty,
        Unit = e.Unit,
        EntryAt = e.EntryAt,
        ExpiresAt = e.ExpiresAt,
        SiteId = e.SiteId,
        SiteName = e.Site?.Name ?? string.Empty,
        Zone = e.Zone,
        SupplierId = e.SupplierId,
        SupplierName = e.Supplier?.Name,
        CostPerUnit = e.CostPerUnit,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
