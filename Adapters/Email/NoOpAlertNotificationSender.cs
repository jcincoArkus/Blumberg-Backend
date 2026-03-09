using Microsoft.Extensions.Logging;
using Shared.Notifications;

namespace Adapters.Email;

/// <summary>
/// No-op implementation of IAlertNotificationSender. Logs and does not send email. Used when ALERT_EMAIL_PROVIDER is None.
/// </summary>
public class NoOpAlertNotificationSender(ILogger<NoOpAlertNotificationSender> logger) : IAlertNotificationSender
{
    /// <inheritdoc />
    public Task SendAsync(AlertNotificationMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Alert email (no-op): Alert {AlertId}, Severity {Severity}, {RecipientCount} recipient(s). Set ALERT_EMAIL_PROVIDER=Smtp and SMTP_* env vars to send real emails.", message.AlertId, message.Severity, message.RecipientEmails.Count);
        return Task.CompletedTask;
    }
}
