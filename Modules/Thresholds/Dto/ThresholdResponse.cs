namespace Modules.Thresholds.Dto;

/// <summary>
/// Threshold response DTO
/// </summary>
public class ThresholdResponse
{
    /// <summary>Threshold ID</summary>
    public Guid Id { get; set; }

    /// <summary>Minimum value</summary>
    public decimal Min { get; set; }

    /// <summary>Maximum value</summary>
    public decimal Max { get; set; }

    /// <summary>Duration in seconds</summary>
    public int DurationSeconds { get; set; }

    /// <summary>Created at</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Updated at</summary>
    public DateTime? UpdatedAt { get; set; }
}
