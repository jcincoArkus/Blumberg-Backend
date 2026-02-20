using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Ingestion.Service;
using Shared.Constants;

namespace Adapters.Server;

/// <summary>
/// Authenticates requests using an API key in the X-Api-Key header.
/// On success, sets a principal with orgId claim so TenantContext resolves the organization.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private const string ApiKeySchemeName = "ApiKey";
    private readonly IApiKeyService _apiKeyService;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyService apiKeyService)
        : base(options, logger, encoder)
    {
        _apiKeyService = apiKeyService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var headerName = Options.HeaderName;
        var key = Request.Headers[headerName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(key))
            return AuthenticateResult.NoResult();

        var organizationId = await _apiKeyService.ValidateKeyAsync(key, Context.RequestAborted);
        if (organizationId == null)
        {
            Logger.LogWarning("Invalid or revoked API key");
            return AuthenticateResult.Fail("Invalid or revoked API key");
        }

        var claims = new[]
        {
            new Claim(TenantClaimNames.OrgIdClaim, organizationId.Value.ToString()),
            new Claim(ClaimTypes.AuthenticationMethod, ApiKeySchemeName)
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
