namespace Shared.Entity;

/// <summary>
/// Records a single rejected reading within an ingestion run (per-reading result).
/// </summary>
public class IngestionRejectedReading
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Ingestion run this rejection belongs to
    /// </summary>
    public Guid IngestionRunId { get; set; }

    /// <summary>
    /// Navigation to the ingestion run
    /// </summary>
    public IngestionRun IngestionRun { get; set; } = null!;

    /// <summary>
    /// Zero-based index of the reading in the batch
    /// </summary>
    public int RowIndex { get; set; }

    /// <summary>
    /// Sensor ID from the ingestion payload (when available). Enables per-sensor rejection tracing.
    /// </summary>
    public Guid? SensorId { get; set; }

    /// <summary>
    /// Reason the reading was rejected
    /// </summary>
    public string RejectionReason { get; set; } = string.Empty;
}
