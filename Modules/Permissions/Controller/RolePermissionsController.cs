using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Permissions.Dto;
using Modules.Permissions.Service;

namespace Modules.Permissions.Controller;

/// <summary>
/// Controller for managing role-permission assignments
/// </summary>
[ApiController]
[Route("api/roles/{roleName}/permissions")]
[Authorize]
public class RolePermissionsController(IRolePermissionService rolePermissionService) : ControllerBase
{
    /// <summary>
    /// Get all permissions for a role
    /// </summary>
    [HttpGet(Name = "GetRolePermissionsV1")]
    public async Task<ActionResult<RolePermissionsResponse>> GetRolePermissions(string roleName)
    {
        var permissions = await rolePermissionService.GetRolePermissionsAsync(roleName);
        return Ok(new RolePermissionsResponse
        {
            RoleName = roleName,
            Permissions = permissions
        });
    }

    /// <summary>
    /// Assign permissions to a role
    /// </summary>
    [HttpPost(Name = "AssignRolePermissionsV1")]
    public async Task<ActionResult> AssignPermissions(
        string roleName,
        [FromBody] AssignPermissionsToRoleRequest request)
    {
        try
        {
            await rolePermissionService.AssignPermissionsToRoleAsync(roleName, request.Permissions);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove permissions from a role
    /// </summary>
    [HttpDelete(Name = "RemoveRolePermissionsV1")]
    public async Task<ActionResult> RemovePermissions(
        string roleName,
        [FromBody] RemovePermissionsFromRoleRequest request)
    {
        try
        {
            await rolePermissionService.RemovePermissionsFromRoleAsync(roleName, request.Permissions);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Replace all permissions for a role
    /// </summary>
    [HttpPut(Name = "ReplaceRolePermissionsV1")]
    public async Task<ActionResult> ReplacePermissions(
        string roleName,
        [FromBody] AssignPermissionsToRoleRequest request)
    {
        try
        {
            await rolePermissionService.ReplaceRolePermissionsAsync(roleName, request.Permissions);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

