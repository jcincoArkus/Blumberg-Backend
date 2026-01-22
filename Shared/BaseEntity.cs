namespace Shared;

/// <summary>
/// Base entity class for all database entities
/// </summary>
/// <remarks>
/// Provides common properties for all entities including:
/// - Id (Guid primary key)
/// - CreatedAt (timestamp)
/// - UpdatedAt (timestamp)
/// - DeletedAt (soft delete timestamp)
/// </remarks>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Timestamp when the entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the entity was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Timestamp when the entity was soft deleted (null if active)
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}

