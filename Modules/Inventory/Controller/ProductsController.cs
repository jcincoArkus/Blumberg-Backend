using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Dto;
using Modules.Inventory.Service;
using Shared.Entity;

namespace Modules.Inventory.Controller;

[ApiController]
[Route("api/v1/inventory/products")]
[Tags("Inventory - Products")]
[Authorize]
public class ProductsController(IProductService service, ILogger<ProductsController> logger) : ControllerBase
{
    [HttpGet(Name = "GetAllProductsV1")]
    [ProducesResponseType(typeof(PagedResponse<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetAll([FromQuery] GetProductsRequest request)
    {
        try
        {
            var (items, totalCount) = await service.GetAllAsync(request);
            return Ok(new PagedResponse<ProductResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting products");
            return StatusCode(500, new { message = "An error occurred while getting products" });
        }
    }

    [HttpGet("{id}", Name = "GetProductByIdV1")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id)
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
            logger.LogError(ex, "Error getting product {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting product" });
        }
    }

    [HttpPost(Name = "CreateProductV1")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] ProductRequest request)
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
            logger.LogError(ex, "Error creating product");
            return StatusCode(500, new { message = "An error occurred while creating product" });
        }
    }

    [HttpPut("{id}", Name = "UpdateProductV1")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, [FromBody] ProductRequest request)
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
            logger.LogError(ex, "Error updating product {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating product" });
        }
    }

    [HttpDelete("{id}", Name = "DeleteProductV1")]
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
            logger.LogError(ex, "Error deleting product {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting product" });
        }
    }

    private static ProductResponse MapToResponse(InventoryProduct e) => new()
    {
        Id = e.Id,
        Sku = e.Sku,
        Name = e.Name,
        CategoryId = e.CategoryId,
        CategoryName = e.Category?.Name ?? string.Empty,
        Unit = e.Unit,
        KgPerBox = e.KgPerBox,
        ShelfLifeDays = e.ShelfLifeDays,
        Price = e.Price,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt ?? e.CreatedAt
    };
}
