using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Adapters.Permissions;
using System.Security.Claims;

namespace Adapters.Server.Attributes;

/// <summary>
/// Authorization attribute that requires a specific permission to access an endpoint
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _resource;
    private readonly string _action;

    /// <summary>
    /// Creates a new RequirePermission attribute
    /// </summary>
    /// <param name="resource">The resource being accessed</param>
    /// <param name="action">The action being performed</param>
    public RequirePermissionAttribute(string resource, string action)
    {
        _resource = resource ?? throw new ArgumentNullException(nameof(resource));
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    /// Called early in the filter pipeline to confirm request is authorized
    /// </summary>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Get Casbin service
        var casbinService = context.HttpContext.RequestServices.GetService<ICasbinAuthorizationService>();
        if (casbinService == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        // Get current user email from claims
        var userEmail = context.HttpContext.User?.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Check permission
        var hasPermission = await casbinService.CheckPermissionAsync(userEmail, _resource, _action);
        if (!hasPermission)
        {
            context.Result = new ForbidResult();
            return;
        }

        // Permission granted - continue
    }
}

