using System.ComponentModel.DataAnnotations;

namespace Modules.Thresholds.Dto;

/// <summary>
/// Threshold request DTO
/// </summary>
public class ThresholdRequest
{
    /// <summary>Minimum value</summary>
    /// <example>0</example>
    [Required]
    public decimal Min { get; set; }

    /// <summary>Maximum value</summary>
    /// <example>100</example>
    [Required]
    public decimal Max { get; set; }

    /// <summary>Duration in seconds (how long the value must be outside min/max to trigger)</summary>
    /// <example>300</example>
    [Required]
    [Range(0, int.MaxValue)]
    public int DurationSeconds { get; set; }
}
