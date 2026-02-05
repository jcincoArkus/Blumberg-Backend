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
    /// <summary>
    /// Gets a list of all active admins
    /// </summary>
    /// <returns>List of admin information</returns>
    /// <response code="200">Returns the list of admins</response>
    /// <response code="500">Internal server error</response>
    [HttpGet(Name = "GetAllAdminsV1")]
    [ProducesResponseType(typeof(List<AdminResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AdminResponse>>> GetAll()
    {
        logger.LogDebug("Getting all admins");

        try
        {
            var admins = await adminService.GetAllAsync();
            logger.LogInformation("Retrieved {Count} admins", admins.Count);
            return Ok(admins);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving admins");
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
    [HttpGet("{id}", Name = "GetAdminByIdV1")]
    [ProducesResponseType(typeof(AdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminResponse>> GetById(Guid id)
    {
        logger.LogDebug("Getting admin by ID: {Id}", id);

        try
        {
            var admin = await adminService.GetByIdAsync(id);
            logger.LogInformation("Retrieved admin {Id}", id);
            return Ok(admin);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Admin not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving admin {Id}", id);
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
    [HttpPost(Name = "CreateAdminV1")]
    [ProducesResponseType(typeof(AdminResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminResponse>> Create([FromBody] AdminRequest admin)
    {
        logger.LogDebug("Creating new admin with email: {Email}", admin.Email);

        try
        {
            var newAdmin = await adminService.CreateAsync(admin);
            logger.LogInformation("Admin created successfully with ID: {Id}", newAdmin.Id);
            return CreatedAtAction(nameof(GetById), new { id = newAdmin.Id }, newAdmin);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Failed to create admin: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating admin with email: {Email}", admin.Email);
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
    [HttpPut("{id}", Name = "UpdateAdminV1")]
    [ProducesResponseType(typeof(AdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminResponse>> Update(Guid id, [FromBody] AdminRequest admin)
    {
        logger.LogDebug("Updating admin {Id}", id);

        try
        {
            var updatedAdmin = await adminService.UpdateAsync(id, admin);
            logger.LogInformation("Admin {Id} updated successfully", id);
            return Ok(updatedAdmin);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Admin not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Failed to update admin {Id}: {Message}", id, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating admin {Id}", id);
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
    [HttpDelete("{id}", Name = "DeleteAdminV1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        logger.LogDebug("Deleting admin {Id}", id);

        try
        {
            await adminService.DeleteAsync(id);
            logger.LogInformation("Admin {Id} deleted successfully", id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning("Admin not found with ID: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting admin {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting admin" });
        }
    }
}
