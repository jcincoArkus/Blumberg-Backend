using Shared.Enums;

namespace Modules.Ingestion.Dto;

/// <summary>
/// Ingestion run summary for list endpoint
/// </summary>
public class IngestionRunListResponse
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
}
