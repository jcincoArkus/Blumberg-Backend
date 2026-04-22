using Shared.Entity;

namespace Shared.Notifications;

/// <summary>
/// Orchestrates "alert became Active" notification: resolve recipients, build message, send via IAlertNotificationSender.
/// Implemented in Alerts module; Ingestion (and others) depend only on this interface.
/// </summary>
public interface IAlertTriggeredNotifier
{
    /// <summary>
    /// Notifies configured recipients about a newly triggered (Active) alert. Fire-and-forget or await with timeout.
    /// </summary>
    Task NotifyTriggeredAsync(Alert alert, CancellationToken cancellationToken = default);
}
