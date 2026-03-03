namespace Modules.Alerts.Dto;

/// <summary>
/// Response DTO for a single alert lifecycle event
/// </summary>
public class AlertEventResponse
{
    /// <summary>Unique event identifier</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>Type of lifecycle event (e.g. Triggered, Acknowledged, Resolved)</summary>
    /// <example>Triggered</example>
    public string EventType { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the event occurred</summary>
    /// <example>2026-02-26T21:52:11Z</example>
    public DateTime OccurredAt { get; set; }

    /// <summary>Human-readable description of the event</summary>
    /// <example>Alert triggered</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>Optional ID of the user who performed the action (null for system events)</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid? ActorId { get; set; }
}
