using Shared.Enums;

namespace Shared.Entity;

/// <summary>
/// A single event in an alert's lifecycle (triggered, acknowledged, resolved, note, etc.).
/// Append-only; used for events history / audit.
/// </summary>
public class AlertEvent : BaseEntity
{
    /// <summary>
    /// Alert this event belongs to
    /// </summary>
    public Guid AlertId { get; set; }

    /// <summary>
    /// Navigation to the alert
    /// </summary>
    public Alert Alert { get; set; } = null!;

    /// <summary>
    /// Type of event
    /// </summary>
    public AlertEventType EventType { get; set; }

    /// <summary>
    /// UTC timestamp when the event occurred (e.g. when alert was triggered/resolved)
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// Human-readable description of the event
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional: user/admin who performed the action (null for system events)
    /// </summary>
    public Guid? ActorId { get; set; }

    /// <summary>
    /// Organization (tenant) for filtering; denormalized from Alert
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Navigation to the organization
    /// </summary>
    public Organization Organization { get; set; } = null!;
}
