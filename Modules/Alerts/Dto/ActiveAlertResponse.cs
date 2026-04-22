namespace Modules.Alerts.Dto;

/// <summary>
/// Dashboard-focused response DTO for an active (unresolved) alert
/// </summary>
public class ActiveAlertResponse
{
    public Guid Id { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? SensorSerial { get; set; }
    public string? SensorTypeName { get; set; }
    public string? EquipmentName { get; set; }
    public DateTime TriggeredAt { get; set; }
    /// <summary>Seconds elapsed since the alert was triggered</summary>
    public double DurationSeconds { get; set; }
}
