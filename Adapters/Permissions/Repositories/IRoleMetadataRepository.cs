using Shared.Entity;

namespace Adapters.Permissions.Repositories;

/// <summary>
/// Repository interface for role metadata operations
/// </summary>
public interface IRoleMetadataRepository
{
    /// <summary>
    /// Gets metadata for a specific role by name
    /// </summary>
    Task<RoleMetadata?> GetRoleMetadataAsync(string roleName);

    /// <summary>
    /// Gets all role metadata
    /// </summary>
    Task<List<RoleMetadata>> GetAllRoleMetadataAsync();

    /// <summary>
    /// Creates or updates role metadata
    /// </summary>
    Task UpsertRoleMetadataAsync(RoleMetadata metadata);

    /// <summary>
    /// Loads role metadata from JSON configuration file
    /// </summary>
    Task LoadRolesFromConfigAsync();

    /// <summary>
    /// Gets the default role for new users
    /// </summary>
    Task<RoleMetadata?> GetDefaultRoleAsync();

    /// <summary>
    /// Delete role metadata by name
    /// </summary>
    Task DeleteRoleMetadataAsync(string roleName);
}

