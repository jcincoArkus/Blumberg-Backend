namespace Shared.Entity;

/// <summary>
/// Threshold entity representing a threshold value
/// </summary>
public class Threshold : BaseEntity
{
    /// <summary>
    /// Threshold's value
    /// </summary>
    public decimal Min { get; set; }

    /// <summary>
    /// Threshold's maximum value
    /// </summary>
    public decimal Max { get; set; }

    /// <summary>
    /// Threshold duration 
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Sensor configured by this threshold
    /// </summary>
    public Sensor? Sensor { get; set; }
}