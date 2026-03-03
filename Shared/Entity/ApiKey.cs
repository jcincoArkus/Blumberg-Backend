namespace Shared.Entity;

/// <summary>
/// API key for machine-to-machine auth (e.g. ingestion). Scoped to an organization.
/// Key hash is stored; raw key is shown only once at creation.
/// </summary>
public class ApiKey
{
    /// <summary>Unique identifier</summary>
    public Guid Id { get; set; }

    /// <summary>SHA256 hash of the raw key (e.g. blumberg_xxx). Never store the raw key.</summary>
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>Organization this key belongs to</summary>
    public Guid OrganizationId { get; set; }

    /// <summary>Organization navigation</summary>
    public Organization Organization { get; set; } = null!;

    /// <summary>Human-readable name (e.g. "Gateway A")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>When the key was created</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>When the key was revoked (null = active)</summary>
    public DateTime? RevokedAt { get; set; }
}
