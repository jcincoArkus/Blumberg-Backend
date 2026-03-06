namespace Shared.Notifications;

/// <summary>
/// Sends an alert-triggered notification (e.g. email). Strategy interface: implementations can use SMTP, AWS SES, no-op, etc.
/// </summary>
public interface IAlertNotificationSender
{
    /// <summary>
    /// Sends the notification for a newly triggered alert. Caller provides resolved recipients and formatted message data.
    /// </summary>
    Task SendAsync(AlertNotificationMessage message, CancellationToken cancellationToken = default);
}
