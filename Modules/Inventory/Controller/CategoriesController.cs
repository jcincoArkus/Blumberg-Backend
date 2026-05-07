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
[Route("api/v1/inventory/categories")]
[Tags("Inventory - Categories")]
[Authorize]
public class CategoriesController(ICategoryService service, ILogger<CategoriesController> logger) : ControllerBase
{
    [HttpGet(Name = "GetAllCategoriesV1")]
    [ProducesResponseType(typeof(PagedResponse<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<CategoryResponse>>> GetAll([FromQuery] PaginationRequest request)
    {
        logger.LogDebug("Getting all categories");
        try
        {
            var (items, totalCount) = await service.GetAllAsync(request);
            return Ok(new PagedResponse<CategoryResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting categories");
            return StatusCode(500, new { message = "An error occurred while getting categories" });
        }
    }

    [HttpGet("{id}", Name = "GetCategoryByIdV1")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> GetById(Guid id)
    {
        try
        {
            var entity = await service.GetByIdAsync(id);
            return Ok(MapToResponse(entity));
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Category not found: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting category {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting category" });
        }
    }

    [HttpPost(Name = "CreateCategoryV1")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryResponse>> Create([FromBody] CategoryRequest request)
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
            logger.LogError(ex, "Error creating category");
            return StatusCode(500, new { message = "An error occurred while creating category" });
        }
    }

    [HttpPut("{id}", Name = "UpdateCategoryV1")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> Update(Guid id, [FromBody] CategoryRequest request)
    {
        try
        {
            var entity = await service.UpdateAsync(id, request);
            return Ok(MapToResponse(entity));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating category {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating category" });
        }
    }

    [HttpDelete("{id}", Name = "DeleteCategoryV1")]
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
            logger.LogError(ex, "Error deleting category {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting category" });
        }
    }

    private static CategoryResponse MapToResponse(InventoryCategory e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Color = e.Color,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt ?? e.CreatedAt
    };
}
