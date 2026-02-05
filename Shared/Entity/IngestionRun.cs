namespace Shared.Entity;

/// <summary>
/// Ingestion run entity representing a batch of sensor readings ingested together
/// </summary>
public class IngestionRun : BaseEntity
{
    /// <summary>
    /// Sensor readings produced by this run
    /// </summary>
    public ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
}
