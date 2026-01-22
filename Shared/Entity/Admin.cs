namespace Shared.Entity;

/// <summary>
/// Admin entity representing system administrators
/// </summary>
public class Admin : BaseEntity
{
    /// <summary>
    /// Admin's email address (unique)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password for authentication
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Admin's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Admin's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;
}

