namespace Shared.Enums;

/// <summary>
/// Status of an ingestion job.
/// Stored as string in database (VARCHAR 50); adding new values does not require a migration.
/// </summary>
public enum IngestionStatus
{
    /// <summary>Ingestion is pending.</summary>
    Pending,

    /// <summary>Ingestion is in progress.</summary>
    InProgress,

    /// <summary>Ingestion completed successfully.</summary>
    Success,

    /// <summary>Ingestion completed with partial success.</summary>
    PartialSuccess,

    /// <summary>Ingestion failed.</summary>
    Failed
}
