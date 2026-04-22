namespace Modules.Admins.Dto;

/// <summary>
/// Response DTO for admin information (excludes sensitive data)
/// </summary>
public class AdminResponse
{
    /// <summary>
    /// Admin unique identifier
    /// </summary>
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Admin email address
    /// </summary>
    /// <example>admin@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Admin first name
    /// </summary>
    /// <example>John</example>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Admin last name
    /// </summary>
    /// <example>Doe</example>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the admin was created
    /// </summary>
    /// <example>2025-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the admin was last updated
    /// </summary>
    /// <example>2025-01-16T14:00:00Z</example>
    public DateTime? UpdatedAt { get; set; }
}
