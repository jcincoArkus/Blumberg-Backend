namespace Modules.Equipment.Dto;

/// <summary>
/// Equipment response DTO (no nested entities to avoid JSON cycles)
/// </summary>
public class EquipmentResponse
{
    /// <summary>
    /// Equipment ID
    /// </summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Equipment name
    /// </summary>
    /// <example>Boiler Unit A</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Equipment type
    /// </summary>
    /// <example>Boiler</example>
    public string EquipmentType { get; set; } = string.Empty;

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
    /// Site ID
    /// </summary>
    /// <example>c3d4e5f6-a7b8-9012-cdef-123456789012</example>
    public Guid SiteId { get; set; }

    /// <summary>
    /// Site name (for display)
    /// </summary>
    /// <example>Main Office</example>
    public string SiteName { get; set; } = string.Empty;

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
