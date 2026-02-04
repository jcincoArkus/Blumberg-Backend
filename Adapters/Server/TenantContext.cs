using Microsoft.AspNetCore.Http;
using Shared.Abstractions;
using Shared.Constants;

namespace Adapters.Server;

/// <summary>
/// Resolves current tenant (organization) from JWT claim or X-Organization-Id header (for dev).
/// </summary>
public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid? CurrentOrganizationId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                return null;

            // Dev: allow X-Organization-Id header to override or set tenant
            var header = context.Request.Headers[TenantClaimNames.OrganizationIdHeader].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(header) && Guid.TryParse(header, out var headerOrgId))
                return headerOrgId;

            // From JWT claim (set after authentication)
            var claim = context.User?.FindFirst(TenantClaimNames.OrgIdClaim)?.Value;
            if (!string.IsNullOrWhiteSpace(claim) && Guid.TryParse(claim, out var claimOrgId))
                return claimOrgId;

            return null;
        }
    }
}
