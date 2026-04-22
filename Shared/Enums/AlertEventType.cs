namespace Shared.Enums;

/// <summary>
/// Type of event in an alert's lifecycle (for events history / audit)
/// </summary>
public enum AlertEventType
{
    Triggered,
    Acknowledged,
    Resolved,
    Note,
    SystemUpdate,
    /// <summary>Email (or other channel) notification was sent for this alert.</summary>
    NotificationSent
}
