namespace Shared.Notifications;

/// <summary>
/// Resolves recipient email addresses for alert notifications (e.g. by organization). Abstraction so Alerts module does not depend on Auth.
/// </summary>
public interface IAlertRecipientResolver
{
    /// <summary>
    /// Returns email addresses that should receive notifications for alerts in the given organization (e.g. org admins).
    /// </summary>
    Task<IReadOnlyList<string>> GetEmailsForOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
}
