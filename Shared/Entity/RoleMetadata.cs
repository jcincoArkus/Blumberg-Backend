using Shared.ValueObjects;

namespace Shared.Entity;

/// <summary>
/// Entity representing metadata for RBAC roles
/// Used for UI display and role management
/// </summary>
public class RoleMetadata
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Role name (matches Casbin role identifier)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display name for UI
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Description of role capabilities
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// RGB color for UI display
    /// </summary>
    public byte ColorR { get; set; }
    public byte ColorG { get; set; }
    public byte ColorB { get; set; }

    /// <summary>
    /// Whether this is the default role for new users
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the color as a RoleColor value object
    /// </summary>
    public RoleColor GetColor() => new(ColorR, ColorG, ColorB);

    /// <summary>
    /// Sets the color from a RoleColor value object
    /// </summary>
    public void SetColor(RoleColor color)
    {
        ColorR = color.R;
        ColorG = color.G;
        ColorB = color.B;
    }
}

