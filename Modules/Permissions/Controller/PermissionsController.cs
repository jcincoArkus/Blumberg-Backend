using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Permissions.Dto;
using Modules.Permissions.Service;
using System.Security.Claims;

namespace Modules.Permissions.Controller;

/// <summary>
/// Controller for querying permissions, resources, and actions
/// </summary>
[ApiController]
[Route("api/permissions")]
[Authorize]
public class PermissionsController(IPermissionQueryService permissionQueryService) : ControllerBase
{
    /// <summary>
    /// Get all available resources with their actions
    /// </summary>
    [HttpGet("resources", Name = "GetPermissionResourcesV1")]
    public async Task<ActionResult<ResourcesResponse>> GetResources()
    {
        var resources = await permissionQueryService.GetAllResourcesAsync();
        return Ok(new ResourcesResponse { Resources = resources });
    }

    /// <summary>
    /// Get all available actions
    /// </summary>
    [HttpGet("actions", Name = "GetPermissionActionsV1")]
    public async Task<ActionResult<ActionsResponse>> GetActions()
    {
        var actions = await permissionQueryService.GetAllActionsAsync();
        return Ok(new ActionsResponse { Actions = actions });
    }

    /// <summary>
    /// Check if the current user has a specific permission
    /// </summary>
    [HttpPost("check", Name = "CheckPermissionCurrentUserV1")]
    public async Task<ActionResult<CheckPermissionResponse>> CheckPermission(
        [FromBody] CheckPermissionRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var hasPermission = await permissionQueryService.CheckUserPermissionAsync(
            userId,
            request.Resource,
            request.Action);

        return Ok(new CheckPermissionResponse { HasPermission = hasPermission });
    }

    /// <summary>
    /// Check if a specific user has a permission (admin only)
    /// </summary>
    [HttpPost("check/{userId}", Name = "CheckPermissionUserV1")]
    public async Task<ActionResult<CheckPermissionResponse>> CheckUserPermission(
        string userId,
        [FromBody] CheckPermissionRequest request)
    {
        var hasPermission = await permissionQueryService.CheckUserPermissionAsync(
            userId,
            request.Resource,
            request.Action);

        return Ok(new CheckPermissionResponse { HasPermission = hasPermission });
    }
}

