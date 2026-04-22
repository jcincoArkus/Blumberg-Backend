using Shared.Enums;

namespace Modules.Ingestion.Dto;

/// <summary>
/// Ingestion run detail including per-reading results (accepted/rejected)
/// </summary>
public class IngestionRunDetailResponse
{
    /// <summary>Run ID</summary>
    public Guid Id { get; set; }

    /// <summary>Source of the data</summary>
    public IngestionSource Source { get; set; }

    /// <summary>Run status</summary>
    public IngestionStatus Status { get; set; }

    /// <summary>Total records in batch</summary>
    public int TotalRecords { get; set; }

    /// <summary>Accepted count</summary>
    public int AcceptedRecords { get; set; }

    /// <summary>Rejected count</summary>
    public int RejectedRecords { get; set; }

    /// <summary>When the run started</summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>When the run completed</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Organization ID</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>When the run was created</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Accepted readings (saved to the database)</summary>
    public IReadOnlyList<IngestionReadingResult> AcceptedReadings { get; set; } = [];

    /// <summary>Rejected readings with reason per row</summary>
    public IReadOnlyList<RejectedReadingResult> RejectedReadings { get; set; } = [];
}

/// <summary>
/// A single accepted reading in an ingestion run (minimal view for run detail)
/// </summary>
public class IngestionReadingResult
{
    /// <summary>Reading ID</summary>
    public Guid Id { get; set; }

    /// <summary>Sensor ID</summary>
    public Guid SensorId { get; set; }

    /// <summary>Value</summary>
    public decimal Value { get; set; }

    /// <summary>Timestamp UTC</summary>
    public DateTime TimestampUtc { get; set; }

    /// <summary>Unit</summary>
    public string Unit { get; set; } = string.Empty;
}
