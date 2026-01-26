namespace Modules.Admins.Dto;

/// <summary>
/// Response DTO for admin information (excludes sensitive data)
/// </summary>
public class AdminResponse
{
    /// <summary>
    /// Admin unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Admin email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Admin first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Admin last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the admin was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the admin was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
