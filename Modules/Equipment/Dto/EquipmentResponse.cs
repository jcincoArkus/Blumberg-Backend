namespace Modules.Equipment.Dto;

/// <summary>
/// Equipment response DTO (no nested entities to avoid JSON cycles)
/// </summary>
public class EquipmentResponse
{
    /// <summary>
    /// Equipment ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Equipment name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Equipment type
    /// </summary>
    public string EquipmentType { get; set; } = string.Empty;

    /// <summary>
    /// Organization ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// Organization name (for display)
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// Site ID
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// Site name (for display)
    /// </summary>
    public string SiteName { get; set; } = string.Empty;

    /// <summary>
    /// Created at
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Updated at
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
