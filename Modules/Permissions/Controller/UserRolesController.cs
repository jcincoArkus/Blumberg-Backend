using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Permissions.Dto;
using Modules.Permissions.Service;

namespace Modules.Permissions.Controller;

/// <summary>
/// Controller for managing user-role assignments
/// </summary>
[ApiController]
[Route("api/users/{userId}/roles")]
[Authorize]
public class UserRolesController(IUserRoleService userRoleService) : ControllerBase
{
    /// <summary>
    /// Get all roles for a user
    /// </summary>
    [HttpGet(Name = "GetUserRolesV1")]
    public async Task<ActionResult<UserRolesResponse>> GetUserRoles(string userId)
    {
        var roles = await userRoleService.GetUserRolesAsync(userId);
        return Ok(new UserRolesResponse
        {
            UserId = userId,
            Roles = roles
        });
    }

    /// <summary>
    /// Assign roles to a user
    /// </summary>
    [HttpPost(Name = "AssignUserRolesV1")]
    public async Task<ActionResult> AssignRoles(
        string userId,
        [FromBody] AssignRolesToUserRequest request)
    {
        try
        {
            await userRoleService.AssignRolesToUserAsync(userId, request.Roles);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove roles from a user
    /// </summary>
    [HttpDelete(Name = "RemoveUserRolesV1")]
    public async Task<ActionResult> RemoveRoles(
        string userId,
        [FromBody] RemoveRolesFromUserRequest request)
    {
        try
        {
            await userRoleService.RemoveRolesFromUserAsync(userId, request.Roles);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Replace all roles for a user
    /// </summary>
    [HttpPut(Name = "ReplaceUserRolesV1")]
    public async Task<ActionResult> ReplaceRoles(
        string userId,
        [FromBody] AssignRolesToUserRequest request)
    {
        try
        {
            await userRoleService.ReplaceUserRolesAsync(userId, request.Roles);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

/// <summary>
/// Controller for querying users by role
/// </summary>
[ApiController]
[Route("api/roles/{roleName}/users")]
[Authorize]
public class RoleUsersController(IUserRoleService userRoleService) : ControllerBase
{
    /// <summary>
    /// Get all users with a specific role
    /// </summary>
    [HttpGet(Name = "GetRoleUsersV1")]
    public async Task<ActionResult<RoleUsersResponse>> GetRoleUsers(string roleName)
    {
        var users = await userRoleService.GetUsersForRoleAsync(roleName);
        return Ok(new RoleUsersResponse
        {
            RoleName = roleName,
            UserIds = users
        });
    }
}

