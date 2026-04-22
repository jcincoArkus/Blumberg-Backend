namespace Modules.Ingestion.Dto;

/// <summary>
/// Aggregated ingestion stats for a time range (e.g. last 24h)
/// </summary>
public class IngestionStatsResponse
{
    /// <summary>Total number of records across all runs in the range</summary>
    /// <example>100</example>
    public int TotalRecords { get; set; }

    /// <summary>Total accepted records</summary>
    /// <example>98</example>
    public int AcceptedRecords { get; set; }

    /// <summary>Total rejected records</summary>
    /// <example>2</example>
    public int RejectedRecords { get; set; }

    /// <summary>Number of distinct rejection reasons in the range (unique error types)</summary>
    /// <example>3</example>
    public int UniqueErrorTypes { get; set; }
}
