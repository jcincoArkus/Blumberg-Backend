using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Permissions.Dto;
using Modules.Permissions.Service;
using Shared.Permissions;

namespace Modules.Permissions.Controller;

/// <summary>
/// Controller for managing roles
/// </summary>
[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController(IRoleService roleService) : ControllerBase
{
    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet(Name = "GetAllRolesV1")]
    public async Task<ActionResult<RolesListResponse>> GetAllRoles()
    {
        var roles = await roleService.GetAllRolesAsync();
        return Ok(new RolesListResponse { Roles = roles });
    }

    /// <summary>
    /// Get a specific role by name
    /// </summary>
    [HttpGet("{roleName}", Name = "GetRoleByNameV1")]
    public async Task<ActionResult<RoleResponse>> GetRole(string roleName)
    {
        var role = await roleService.GetRoleByNameAsync(roleName);
        if (role == null)
        {
            return NotFound(new { message = $"Role '{roleName}' not found" });
        }

        return Ok(new RoleResponse { Role = role });
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost(Name = "CreateRoleV1")]
    public async Task<ActionResult<RoleResponse>> CreateRole([FromBody] CreateRoleRequest request)
    {
        try
        {
            var role = await roleService.CreateRoleAsync(request);
            return CreatedAtAction(
                nameof(GetRole),
                new { roleName = role.Name },
                new RoleResponse { Role = role });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    [HttpPut("{roleName}", Name = "UpdateRoleV1")]
    public async Task<ActionResult<RoleResponse>> UpdateRole(
        string roleName,
        [FromBody] UpdateRoleRequest request)
    {
        try
        {
            var role = await roleService.UpdateRoleAsync(roleName, request);
            return Ok(new RoleResponse { Role = role });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{roleName}", Name = "DeleteRoleV1")]
    public async Task<ActionResult> DeleteRole(string roleName)
    {
        try
        {
            await roleService.DeleteRoleAsync(roleName);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

