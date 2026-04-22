namespace Shared.Constants;

/// <summary>
/// Claim and header names used for tenant (organization) scoping.
/// </summary>
public static class TenantClaimNames
{
    /// <summary>
    /// JWT claim name for organization ID.
    /// </summary>
    public const string OrgIdClaim = "orgId";

    /// <summary>
    /// HTTP header for organization ID (e.g. for dev / API clients).
    /// </summary>
    public const string OrganizationIdHeader = "X-Organization-Id";
}
