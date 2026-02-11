namespace Modules.Ingestion.Dto;

/// <summary>
/// Per-reading result for a rejected item in an ingestion run
/// </summary>
public class RejectedReadingResult
{
    /// <summary>Zero-based index in the batch</summary>
    public int RowIndex { get; set; }

    /// <summary>Reason the reading was rejected</summary>
    public string RejectionReason { get; set; } = string.Empty;
}
