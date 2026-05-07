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
[Route("api/v1/inventory/suppliers")]
[Tags("Inventory - Suppliers")]
[Authorize]
public class SuppliersController(ISupplierService service, ILogger<SuppliersController> logger) : ControllerBase
{
    [HttpGet(Name = "GetAllSuppliersV1")]
    [ProducesResponseType(typeof(PagedResponse<SupplierResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<SupplierResponse>>> GetAll([FromQuery] PaginationRequest request)
    {
        try
        {
            var (items, totalCount) = await service.GetAllAsync(request);
            return Ok(new PagedResponse<SupplierResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting suppliers");
            return StatusCode(500, new { message = "An error occurred while getting suppliers" });
        }
    }

    [HttpGet("{id}", Name = "GetSupplierByIdV1")]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierResponse>> GetById(Guid id)
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
            logger.LogError(ex, "Error getting supplier {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting supplier" });
        }
    }

    [HttpPost(Name = "CreateSupplierV1")]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<SupplierResponse>> Create([FromBody] SupplierRequest request)
    {
        try
        {
            var entity = await service.CreateAsync(request);
            var response = MapToResponse(entity);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating supplier");
            return StatusCode(500, new { message = "An error occurred while creating supplier" });
        }
    }

    [HttpPut("{id}", Name = "UpdateSupplierV1")]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierResponse>> Update(Guid id, [FromBody] SupplierRequest request)
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
            logger.LogError(ex, "Error updating supplier {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating supplier" });
        }
    }

    [HttpDelete("{id}", Name = "DeleteSupplierV1")]
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
            logger.LogError(ex, "Error deleting supplier {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting supplier" });
        }
    }

    private static SupplierResponse MapToResponse(Supplier e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt ?? e.CreatedAt
    };
}
