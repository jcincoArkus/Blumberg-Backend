namespace Modules.Inventory.Dto;

/// <summary>Site zone read model</summary>
public class SiteZoneResponse
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
