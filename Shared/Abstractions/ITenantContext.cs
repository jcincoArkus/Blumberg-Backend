namespace Shared.Abstractions;

/// <summary>
/// Provides the current tenant (organization) ID for the request.
/// Resolved from JWT claim "orgId" or X-Organization-Id header (for dev).
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Current organization ID for the request, or null if not authenticated / no tenant.
    /// </summary>
    Guid? CurrentOrganizationId { get; }
}
