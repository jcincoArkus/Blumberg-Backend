using Shared.Enums;

namespace Shared.Entity;

/// <summary>
/// Ingestion run entity representing a batch of sensor readings ingested together
/// </summary>
public class IngestionRun : BaseEntity
{
    /// <summary>
    /// Source of the ingested data
    /// </summary>
    public IngestionSource Source { get; set; }

    /// <summary>
    /// Current status of the ingestion run
    /// </summary>
    public IngestionStatus Status { get; set; }

    /// <summary>
    /// Total number of records in the batch
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Number of records accepted
    /// </summary>
    public int AcceptedRecords { get; set; }

    /// <summary>
    /// Number of records rejected
    /// </summary>
    public int RejectedRecords { get; set; }

    /// <summary>
    /// When the run started
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// When the run completed
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Organization this run belongs to
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Organization navigation property
    /// </summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>
    /// Sensor readings produced by this run (accepted)
    /// </summary>
    public ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();

    /// <summary>
    /// Rejected readings in this run (per-reading results)
    /// </summary>
    public ICollection<IngestionRejectedReading> RejectedReadings { get; set; } = new List<IngestionRejectedReading>();
}
