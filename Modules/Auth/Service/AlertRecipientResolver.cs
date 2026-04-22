using Microsoft.Extensions.Logging;
using Modules.Auth.Repository;
using Shared.Notifications;

namespace Modules.Auth.Service;

/// <summary>
/// Resolves alert notification recipients by organization (org admins). Implements Shared.Notifications.IAlertRecipientResolver.
/// </summary>
public class AlertRecipientResolver(IAdminRepository adminRepository, ILogger<AlertRecipientResolver> logger) : IAlertRecipientResolver
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetEmailsForOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var emails = await adminRepository.GetEmailsByOrganizationAsync(organizationId, cancellationToken);
        logger.LogDebug("Resolved {Count} recipient(s) for organization {OrgId}", emails.Count, organizationId);
        return emails;
    }
}
