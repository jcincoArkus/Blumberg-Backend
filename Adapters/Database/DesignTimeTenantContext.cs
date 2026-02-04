using Shared.Abstractions;

namespace Adapters.Database;

/// <summary>
/// Tenant context for design-time (migrations, tooling). No request scope; CurrentOrganizationId is always null.
/// </summary>
internal sealed class DesignTimeTenantContext : ITenantContext
{
    public Guid? CurrentOrganizationId => null;
}
