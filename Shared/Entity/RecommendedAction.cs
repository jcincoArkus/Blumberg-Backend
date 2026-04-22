using Shared.Enums;

namespace Shared.Entity;

/// <summary>
/// Predefined recommended action for alerts, keyed by sensor type and severity.
/// Only active actions are returned for an alert; display order is used for ordering.
/// </summary>
public class RecommendedAction : BaseEntity
{
    /// <summary>Sensor type this action applies to.</summary>
    public Guid SensorTypeId { get; set; }

    /// <summary>Navigation to sensor type.</summary>
    public SensorType SensorType { get; set; } = null!;

    /// <summary>Severity level (Critical, Warning, Info). Only actions matching the alert's severity are returned.</summary>
    public AlertSeverity Severity { get; set; }

    /// <summary>Short title (e.g. "Call Location").</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Description or instructions (e.g. "Open the remote control for the sensor to adjust.").</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Display order (lower = first).</summary>
    public int DisplayOrder { get; set; }

    /// <summary>When false, this action is not returned for alerts (enable/disable without deleting).</summary>
    public bool IsActive { get; set; } = true;
}
