using Shared.Enums;

namespace Modules.Sensors.Dto;

/// <summary>
/// Sensor response DTO (no nested entities to avoid JSON cycles)
/// </summary>
public class SensorResponse
{
    /// <summary>
    /// Sensor ID
    /// </summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Sensor serial number
    /// </summary>
    /// <example>SN-001-2025</example>
    public string Serial { get; set; } = string.Empty;

    /// <summary>
    /// Sensor operational status
    /// </summary>
    /// <example>Active</example>
    public SensorStatus Status { get; set; }

    /// <summary>
    /// Organization ID
    /// </summary>
    /// <example>b2c3d4e5-f6a7-8901-bcde-f12345678901</example>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Organization name (for display)
    /// </summary>
    /// <example>Acme Corp</example>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Equipment ID
    /// </summary>
    /// <example>c3d4e5f6-a7b8-9012-cdef-123456789012</example>
    public Guid EquipmentId { get; set; }

    /// <summary>
    /// Equipment name (for display)
    /// </summary>
    /// <example>Boiler Unit A</example>
    public string EquipmentName { get; set; } = string.Empty;

    /// <summary>
    /// Sensor type ID
    /// </summary>
    /// <example>d4e5f6a7-b890-1234-defg-234567890123</example>
    public Guid SensorTypeId { get; set; }

    /// <summary>
    /// Sensor type name (for display)
    /// </summary>
    /// <example>Temperature</example>
    public string SensorTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Threshold ID
    /// </summary>
    /// <example>e5f6a7b8-9012-3456-efgh-345678901234</example>
    public Guid ThresholdId { get; set; }

    /// <summary>
    /// Created at
    /// </summary>
    /// <example>2025-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Updated at
    /// </summary>
    /// <example>2025-01-16T14:00:00Z</example>
    public DateTime? UpdatedAt { get; set; }
}
