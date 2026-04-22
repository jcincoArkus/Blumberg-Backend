using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Sites.Dto;
using Modules.Sites.Service;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Sites.Controller;

/// <summary>
/// Site controller for managing site operations
/// </summary>
[ApiController]
[Route("api/v1/sites")]
[Tags("Sites")]
[Authorize]
public class SiteController(ISiteService siteService, ILogger<SiteController> logger) : ControllerBase
{
    /// <summary>
    /// Gets paginated sites
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated list of sites</returns>
    [HttpGet(Name = "GetAllSitesV1")]
    [ProducesResponseType(typeof(PagedResponse<SiteResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<SiteResponse>>> GetAll([FromQuery] PaginationRequest request)
    {
        logger.LogDebug("Getting all sites");

        try
        {
            var (items, totalCount) = await siteService.GetAllAsync(request);
            logger.LogInformation("Retrieved {Count} sites", items.Count);
            return Ok(new PagedResponse<SiteResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all sites");
            return StatusCode(500, new { message = "An error occurred while getting all sites" });
        }
    }

    /// <summary>
    /// Gets a site by ID
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <returns>Site</returns>
    [HttpGet("{id}", Name = "GetSiteByIdV1")]
    [ProducesResponseType(typeof(SiteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteResponse>> GetById(Guid id)
    {
        logger.LogDebug("Getting site by ID: {Id}", id);

        try
        {
            var site = await siteService.GetByIdAsync(id);
            logger.LogInformation("Retrieved site {Id}", id);
            return Ok(MapToResponse(site));
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Site not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting site {Id}", id);
            return StatusCode(500, new { message = "An error occurred while getting site by ID" });
        }
    }

    /// <summary>
    /// Creates a new site
    /// </summary>
    /// <param name="site">Site information</param>
    /// <returns>Site</returns>
    [HttpPost(Name = "CreateSiteV1")]
    [ProducesResponseType(typeof(SiteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SiteResponse>> Create([FromBody] SiteRequest site)
    {
        logger.LogDebug("Creating new site: {Name}", site.Name);

        try
        {
            var newSite = await siteService.CreateAsync(site);
            var response = MapToResponse(newSite);
            logger.LogInformation("Site created successfully with ID: {Id}", response.Id);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Failed to create site: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating site: {Name}", site.Name);
            return StatusCode(500, new { message = "An error occurred while creating site" });
        }
    }

    /// <summary>
    /// Updates a site
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <param name="site">Site information</param>
    /// <returns>Site</returns>
    [HttpPut("{id}", Name = "UpdateSiteV1")]
    [ProducesResponseType(typeof(SiteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteResponse>> Update(Guid id, [FromBody] SiteRequest site)
    {
        logger.LogDebug("Updating site {Id}", id);

        try
        {
            var updatedSite = await siteService.UpdateAsync(id, site);
            logger.LogInformation("Site {Id} updated successfully", id);
            return Ok(MapToResponse(updatedSite));
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Site not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Failed to update site {Id}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating site {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating site" });
        }
    }

    /// <summary>
    /// Soft deletes a site
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}", Name = "DeleteSiteV1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        logger.LogDebug("Deleting site {Id}", id);

        try
        {
            await siteService.DeleteAsync(id);
            logger.LogInformation("Site {Id} deleted successfully", id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Site not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting site {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting site" });
        }
    }

    /// <summary>
    /// Maps a Site entity to a SiteResponse DTO
    /// </summary>
    private static SiteResponse MapToResponse(Site site)
    {
        return new SiteResponse
        {
            Id = site.Id,
            Name = site.Name,
            Address = site.Address,
            City = site.City,
            State = site.State,
            PostalCode = site.PostalCode,
            Country = site.Country,
            OrganizationId = site.OrganizationId,
            OrganizationName = site.Organization?.Name ?? string.Empty,
            CreatedAt = site.CreatedAt,
            UpdatedAt = site.UpdatedAt
        };
    }
}
