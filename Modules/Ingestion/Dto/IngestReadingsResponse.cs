namespace Modules.Ingestion.Dto;

/// <summary>
/// Response after processing a batch of readings
/// </summary>
public class IngestReadingsResponse
{
    /// <summary>Ingestion run ID</summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid RunId { get; set; }

    /// <summary>Total number of records in the batch</summary>
    /// <example>100</example>
    public int TotalRecords { get; set; }

    /// <summary>Number of records accepted</summary>
    /// <example>95</example>
    public int AcceptedRecords { get; set; }

    /// <summary>Number of records rejected</summary>
    /// <example>5</example>
    public int RejectedRecords { get; set; }

    /// <summary>Final run status</summary>
    /// <example>PartialSuccess</example>
    public string Status { get; set; } = string.Empty;
}
