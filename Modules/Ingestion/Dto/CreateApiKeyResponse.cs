namespace Modules.Ingestion.Dto;

/// <summary>
/// Response after creating an API key. The raw key is returned only once.
/// </summary>
public class CreateApiKeyResponse
{
    /// <summary>Key ID</summary>
    public Guid Id { get; set; }

    /// <summary>Name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Organization ID</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>When the key was created</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Raw API key — show once and store securely; cannot be retrieved later</summary>
    public string Key { get; set; } = string.Empty;
}
