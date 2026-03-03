using Shared.Enums;

namespace Shared.Entity;

/// <summary>
/// Alert entity representing an out-of-range sensor reading event
/// </summary>
public class Alert : BaseEntity
{
    /// <summary>
    /// Sensor that triggered this alert
    /// </summary>
    public Guid SensorId { get; set; }

    /// <summary>
    /// Navigation to the sensor
    /// </summary>
    public Sensor Sensor { get; set; } = null!;

    /// <summary>
    /// Equipment containing the sensor (denormalized for efficient querying)
    /// </summary>
    public Guid EquipmentId { get; set; }

    /// <summary>
    /// Navigation to the equipment
    /// </summary>
    public Equipment Equipment { get; set; } = null!;

    /// <summary>
    /// Site containing the equipment (denormalized for efficient querying)
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// Navigation to the site
    /// </summary>
    public Site Site { get; set; } = null!;

    /// <summary>
    /// Organization this alert belongs to (tenant scope)
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Navigation to the organization
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// Severity level of this alert
    /// </summary>
    public AlertSeverity Severity { get; set; }

    /// <summary>
    /// The sensor reading value that triggered this alert
    /// </summary>
    public decimal TriggeredValue { get; set; }

    /// <summary>
    /// Threshold minimum at the time of alert creation
    /// </summary>
    public decimal ThresholdMin { get; set; }

    /// <summary>
    /// Threshold maximum at the time of alert creation
    /// </summary>
    public decimal ThresholdMax { get; set; }

    /// <summary>
    /// UTC timestamp when the alert was triggered
    /// </summary>
    public DateTime TriggeredAt { get; set; }

    /// <summary>
    /// Current lifecycle status of this alert
    /// </summary>
    public AlertStatus Status { get; set; }

    /// <summary>
    /// UTC timestamp when the alert was resolved (null if still active)
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Events in this alert's lifecycle (triggered, acknowledged, resolved, notes)
    /// </summary>
    public ICollection<AlertEvent> Events { get; set; } = new List<AlertEvent>();
}
