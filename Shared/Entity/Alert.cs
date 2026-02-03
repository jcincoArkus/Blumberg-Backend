namespace Shared.Entity;

/// <summary>
/// Alert entity representing an alert
/// </summary>
public class Alert : BaseEntity
{
    /// <summary>
    /// Alert's Title   
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Alert's description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Alert's type
    /// </summary>
    public enum AlertType
    {
        Critical,
        Warning,
        Info
    }

    /// <summary>
    /// Alert's status
    /// </summary>
    public enum AlertStatus
    {
        Active,
        Resolved,
        Acknowledged
    }

    /// <summary>
    /// Sensor reading that triggered this alert
    /// </summary>
    public Guid SensorReadingId { get; set; }

    /// <summary>
    /// Navigation to the sensor reading
    /// </summary>
    public SensorReading SensorReading { get; set; } = null!;
}