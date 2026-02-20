namespace Modules.Ingestion.Dto;

/// <summary>
/// API key summary for list (key value is never returned)
/// </summary>
public class ApiKeyListResponse
{
    /// <summary>Key ID</summary>
    public Guid Id { get; set; }

    /// <summary>Name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>When created</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>When revoked (null = active)</summary>
    public DateTime? RevokedAt { get; set; }
}
