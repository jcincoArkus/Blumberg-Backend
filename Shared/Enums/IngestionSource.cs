namespace Shared.Enums;

/// <summary>
/// Source of ingested data (e.g. API, file, simulated).
/// Stored as string in database (VARCHAR 50); adding new values does not require a migration.
/// </summary>
public enum IngestionSource
{
    /// <summary>Data ingested via API.</summary>
    Api,

    /// <summary>Data ingested from CSV file.</summary>
    Csv,

    /// <summary>Data from simulation.</summary>
    Simulated
}
