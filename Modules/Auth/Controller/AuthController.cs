using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Modules.Auth.Dto;
using Modules.Auth.Service;

namespace Modules.Auth.Controller;

/// <summary>
/// Controller for authentication endpoints
/// </summary>
[ApiController]
[Route("/api/v1/auth")]
[Tags("Authentication")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>
    /// Authenticates an admin user
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token and user info</returns>
    /// <response code="200">Login successful</response>
    /// <response code="401">Invalid credentials</response>
    [AllowAnonymous]
    [HttpPost("login", Name = "LoginV1")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        logger.LogDebug("Attempting login for email: {Email}", request.Email);

        try
        {
            var response = await authService.LoginAsync(request);
            logger.LogInformation("Login successful for email: {Email}", request.Email);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Login failed for email: {Email}", request.Email);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token (long-lived session)
    /// </summary>
    /// <param name="request">Refresh token from login or previous refresh</param>
    /// <returns>New access and refresh tokens</returns>
    /// <response code="200">Tokens refreshed</response>
    /// <response code="401">Invalid or expired refresh token</response>
    [AllowAnonymous]
    [HttpPost("refresh", Name = "RefreshV1")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request)
    {
        try
        {
            var response = await authService.RefreshAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Refresh failed: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "An error occurred during refresh" });
        }
    }
}

