namespace Modules.Ingestion.Dto;

/// <summary>
/// Aggregated ingestion stats for a time range (e.g. last 24h)
/// </summary>
public class IngestionStatsResponse
{
    /// <summary>Total number of records across all runs in the range</summary>
    public int TotalRecords { get; set; }

    /// <summary>Total accepted records</summary>
    public int AcceptedRecords { get; set; }

    /// <summary>Total rejected records</summary>
    public int RejectedRecords { get; set; }
}
