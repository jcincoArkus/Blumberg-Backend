namespace Shared.Entity;

/// <summary>
/// Entity for storing Casbin policy rules in PostgreSQL
/// This table stores both policies (p) and role assignments (g)
/// </summary>
public class CasbinRule
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Policy type: "p" for policy, "g" for grouping (role assignment)
    /// </summary>
    public string PType { get; set; } = string.Empty;

    /// <summary>
    /// First parameter (subject/user/role)
    /// For policy (p): role name (e.g., "admin")
    /// For grouping (g): user email (e.g., "user@example.com")
    /// </summary>
    public string? V0 { get; set; }

    /// <summary>
    /// Second parameter (object/resource/role)
    /// For policy (p): resource name (e.g., "equipment")
    /// For grouping (g): role name (e.g., "admin")
    /// </summary>
    public string? V1 { get; set; }

    /// <summary>
    /// Third parameter (action)
    /// For policy (p): action name (e.g., "read", "write")
    /// For grouping (g): null
    /// </summary>
    public string? V2 { get; set; }

    /// <summary>
    /// Fourth parameter (reserved for future use)
    /// </summary>
    public string? V3 { get; set; }

    /// <summary>
    /// Fifth parameter (reserved for future use)
    /// </summary>
    public string? V4 { get; set; }

    /// <summary>
    /// Sixth parameter (reserved for future use)
    /// </summary>
    public string? V5 { get; set; }
}

