using Microsoft.Extensions.Logging;
using Modules.Alerts.Repository;
using Shared.Entity;
using Shared.Enums;
using Shared.Notifications;

namespace Modules.Alerts.Service;

/// <summary>
/// Orchestrates "alert became Active" notification: loads alert with Sensor/Equipment, resolves recipients, builds message, sends via IAlertNotificationSender.
/// </summary>
public class AlertTriggeredNotifier(
    IAlertRepository alertRepository,
    IAlertRecipientResolver recipientResolver,
    IAlertNotificationSender sender,
    ILogger<AlertTriggeredNotifier> logger) : IAlertTriggeredNotifier
{
    /// <inheritdoc />
    public async Task NotifyTriggeredAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        var loaded = await alertRepository.GetByIdAsync(alert.Id);
        if (loaded == null)
        {
            logger.LogWarning("Alert {AlertId} not found for notification", alert.Id);
            return;
        }

        var recipients = await recipientResolver.GetEmailsForOrganizationAsync(loaded.OrganizationId, cancellationToken);
        if (recipients.Count == 0)
        {
            logger.LogWarning("No recipients for organization {OrgId}, skipping alert notification {AlertId}. Set admin emails for this org to receive alert emails.", loaded.OrganizationId, loaded.Id);
            return;
        }

        logger.LogInformation("Sending alert notification for alert {AlertId} to {RecipientCount} recipient(s)", loaded.Id, recipients.Count);

        var message = new AlertNotificationMessage
        {
            AlertId = loaded.Id,
            Severity = loaded.Severity.ToString(),
            SensorName = loaded.Sensor?.Serial ?? "Unknown",
            EquipmentName = loaded.Equipment?.Name ?? "Unknown",
            DetectedValue = loaded.TriggeredValue,
            ThresholdMin = loaded.ThresholdMin,
            ThresholdMax = loaded.ThresholdMax,
            TriggeredAtUtc = loaded.TriggeredAt,
            RecipientEmails = recipients
        };

        await sender.SendAsync(message, cancellationToken);

        await alertRepository.AddEventAsync(new AlertEvent
        {
            AlertId = loaded.Id,
            OrganizationId = loaded.OrganizationId,
            EventType = AlertEventType.NotificationSent,
            OccurredAt = DateTime.UtcNow,
            Description = $"Email notification sent to {recipients.Count} recipient(s)",
        });
    }
}
