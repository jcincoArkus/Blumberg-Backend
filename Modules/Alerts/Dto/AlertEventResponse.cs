namespace Modules.Alerts.Dto;

/// <summary>
/// Response DTO for a single alert lifecycle event
/// </summary>
public class AlertEventResponse
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? ActorId { get; set; }
}
