namespace Shared.Notifications;

/// <summary>
/// Data for an alert-triggered notification (email, etc.). Spec: severity, sensor, equipment, detected value, timestamp.
/// </summary>
public sealed class AlertNotificationMessage
{
    /// <summary>Alert ID (for reference/linking).</summary>
    public Guid AlertId { get; init; }

    /// <summary>Severity (e.g. Critical, Warning).</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Sensor name or serial for display.</summary>
    public string SensorName { get; init; } = string.Empty;

    /// <summary>Equipment name for display.</summary>
    public string EquipmentName { get; init; } = string.Empty;

    /// <summary>Detected value that triggered the alert.</summary>
    public decimal DetectedValue { get; init; }

    /// <summary>Threshold min (for context).</summary>
    public decimal ThresholdMin { get; init; }

    /// <summary>Threshold max (for context).</summary>
    public decimal ThresholdMax { get; init; }

    /// <summary>When the alert was triggered (UTC).</summary>
    public DateTime TriggeredAtUtc { get; init; }

    /// <summary>Recipient email addresses.</summary>
    public IReadOnlyList<string> RecipientEmails { get; init; } = [];
}
