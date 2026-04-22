using Shared.Enums;

namespace Modules.Alerts.Dto;

/// <summary>
/// Response DTO for an alert
/// </summary>
public class AlertResponse
{
    public Guid Id { get; set; }
    public Guid SensorId { get; set; }
    public Guid EquipmentId { get; set; }
    public Guid SiteId { get; set; }
    public string Severity { get; set; } = string.Empty;
    public decimal TriggeredValue { get; set; }
    public decimal ThresholdMin { get; set; }
    public decimal ThresholdMax { get; set; }
    public DateTime TriggeredAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    /// <summary>Display name of the equipment (when loaded)</summary>
    public string? EquipmentName { get; set; }
    /// <summary>Display name of the sensor (when loaded)</summary>
    public string? SensorSerial { get; set; }
    /// <summary>Sensor type kind/category (e.g. Temperature, Humidity) when loaded</summary>
    public string? SensorTypeName { get; set; }
    /// <summary>Lifecycle events (triggered, acknowledged, resolved, etc.) when loaded (e.g. GetById)</summary>
    public List<AlertEventResponse>? Events { get; set; }
    /// <summary>Predefined recommended actions for this alert (sensor type + severity) when loaded (e.g. GetById)</summary>
    public List<RecommendedActionResponse>? RecommendedActions { get; set; }
}

/// <summary>
/// Response DTO for a single recommended action (title, description, display order).
/// </summary>
public class RecommendedActionResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
