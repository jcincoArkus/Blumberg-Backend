using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Adapters.OpenApi;

/// <summary>
/// Operation filter that adds JWT Bearer authentication requirement to Swagger operations
/// that have [Authorize] attribute
/// </summary>
/// <remarks>
/// This filter automatically detects endpoints with [Authorize] attribute and adds
/// the Bearer token requirement in Swagger UI, allowing users to authenticate
/// before testing protected endpoints.
/// </remarks>
public class AuthorizeCheckOperationFilter : IOperationFilter
{
    /// <summary>
    /// Applies the filter to the Swagger operation
    /// </summary>
    /// <param name="operation">The Swagger operation</param>
    /// <param name="context">The operation filter context</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if the controller or action has [Authorize] attribute
        var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>().Any() == true
            || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        // Check if the action has [AllowAnonymous] attribute (overrides [Authorize])
        var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true)
            .OfType<AllowAnonymousAttribute>().Any();

        // If authorized and not explicitly allowed anonymous, add security requirement
        if (hasAuthorize && !hasAllowAnonymous)
        {
            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }
                    ] = []
                }
            ];
        }
    }
}

