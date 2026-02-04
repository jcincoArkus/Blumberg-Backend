using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Admins.Dto;
using Modules.Admins.Service;

namespace Modules.Admins.Controller;

/// <summary>
/// Controller for admin management endpoints
/// </summary>
[ApiController]
[Route("/api/v1/admins")]
[Tags("Admins")]
[Authorize]
public class AdminController(IAdminService adminService, ILogger<AdminController> logger) : ControllerBase
{
    private readonly IAdminService _adminService = adminService;
    private readonly ILogger<AdminController> _logger = logger;

    /// <summary>
    /// Gets a list of all active admins
    /// </summary>
    /// <returns>List of admin information</returns>
    /// <response code="200">Returns the list of admins</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<AdminResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AdminResponse>>> GetAll()
    {
        try
        {
            var admins = await _adminService.GetAllAsync();
            return Ok(admins);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admins");
            return StatusCode(500, new { message = "An error occurred while retrieving admins" });
        }
    }

    /// <summary>
    /// Gets an admin by ID
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <returns>Admin information</returns>
    /// <response code="200">Returns the admin information</response>
    /// <response code="404">Admin not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminResponse>> GetById(Guid id)
    {
        try
        {
            var admin = await _adminService.GetByIdAsync(id);
            return Ok(admin);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Admin not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin");
            return StatusCode(500, new { message = "An error occurred while retrieving admin" });
        }
    }

    /// <summary>
    /// Creates a new admin
    /// </summary>
    /// <param name="admin">Admin information</param>
    /// <returns>Admin information</returns>
    /// <response code="201">Admin created successfully</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost]
    [ProducesResponseType(typeof(AdminResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminResponse>> Create([FromBody] AdminRequest admin)
    {
        try
        {
            var newAdmin = await _adminService.CreateAsync(admin);
            return CreatedAtAction(nameof(GetById), new { id = newAdmin.Id }, newAdmin);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create admin: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin");
            return StatusCode(500, new { message = "An error occurred while creating admin" });
        }
    }

    /// <summary>
    /// Updates an admin
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <param name="admin">Admin information</param>
    /// <returns>Admin information</returns>
    /// <response code="200">Admin updated successfully</response>
    /// <response code="400">Invalid request data</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminResponse>> Update(Guid id, [FromBody] AdminRequest admin)
    {
        try
        {
            var updatedAdmin = await _adminService.UpdateAsync(id, admin);
            return Ok(updatedAdmin);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Admin not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update admin: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin");
            return StatusCode(500, new { message = "An error occurred while updating admin" });
        }
    }

    /// <summary>
    /// Deletes an admin
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <returns>Admin information</returns>
    /// <response code="200">Admin deleted successfully</response>
    /// <response code="404">Admin not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await _adminService.DeleteAsync(id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Admin not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting admin");
            return StatusCode(500, new { message = "An error occurred while deleting admin" });
        }
    }
}
