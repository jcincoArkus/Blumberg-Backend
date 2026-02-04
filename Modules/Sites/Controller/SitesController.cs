using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Sites.Service;
using Modules.Sites.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

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
    private readonly ISiteService _siteService = siteService;
    private readonly ILogger<SiteController> _logger = logger;

    /// <summary>
    /// Gets all sites
    /// </summary>
    /// <returns>List of sites</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<SiteResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SiteResponse>>> GetAll()
    {
        try
        {
            var sites = await _siteService.GetAllAsync();
            return Ok(sites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all sites");
            return StatusCode(500, new { message = "An error occurred while getting all sites" });
        }
    }

    /// <summary>
    /// Gets a site by ID
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <returns>Site</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SiteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteResponse>> GetById(Guid id)
    {
        try
        {
            var site = await _siteService.GetByIdAsync(id);
            return Ok(site);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Site not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting site by ID");
            return StatusCode(500, new { message = "An error occurred while getting site by ID" });
        }
    }

    /// <summary>
        /// Creates a new site
    /// </summary>
    /// <param name="site">Site information</param>
    /// <returns>Site</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SiteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SiteResponse>> Create([FromBody] SiteRequest site)
    {
        try
        {
            var newSite = await _siteService.CreateAsync(site);
            return CreatedAtAction(nameof(GetById), new { id = newSite.Id }, newSite);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create site: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating site");
            return StatusCode(500, new { message = "An error occurred while creating site" });
        }
    }

    /// <summary>
    /// Updates a site
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <param name="site">Site information</param>
    /// <returns>Site</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SiteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteResponse>> Update(Guid id, [FromBody] SiteRequest site)
    {
        try
        {
            var updatedSite = await _siteService.UpdateAsync(id, site);
            return Ok(updatedSite);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Site not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update site: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating site");
            return StatusCode(500, new { message = "An error occurred while updating site" });
        }
    }

    /// <summary>
    /// Soft deletes a site
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await _siteService.DeleteAsync(id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Site not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting site");
            return StatusCode(500, new { message = "An error occurred while deleting site" });
        }
    }
}