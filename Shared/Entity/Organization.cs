namespace Shared.Entity;

/// <summary>
/// Organization entity representing a organization
/// </summary>
public class Organization : BaseEntity
{
    /// <summary>
    /// Organization's name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Organization's slug
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Sites belonging to this organization
    /// </summary>
    public ICollection<Site> Sites { get; set; } = new List<Site>();

    /// <summary>
    /// Equipment belonging to this organization
    /// </summary>
    public ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    /// <summary>
    /// Sensors belonging to this organization
    /// </summary>
    public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();

    /// <summary>
    /// Sensor readings belonging to this organization
    /// </summary>
    public ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();

    /// <summary>
    /// Admins belonging to this organization
    /// </summary>
    public ICollection<Admin> Admins { get; set; } = new List<Admin>();

    /// <summary>
    /// Ingestion runs belonging to this organization
    /// </summary>
    public ICollection<IngestionRun> IngestionRuns { get; set; } = new List<IngestionRun>();
}