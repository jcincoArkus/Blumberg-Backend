using Adapters.Permissions.Repositories;
using Microsoft.Extensions.Logging;
using Modules.Permissions.Dto;
using Shared.ValueObjects;

namespace Modules.Permissions.Service;

/// <summary>
/// Service for managing roles
/// </summary>
public class RoleService(
    IRoleMetadataRepository roleMetadataRepository,
    ILogger<RoleService> logger) : IRoleService
{
    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        logger.LogDebug("Getting all roles");
        var roles = await roleMetadataRepository.GetAllRoleMetadataAsync();
        
        return roles.Select(r => new RoleDto
        {
            Name = r.Name,
            DisplayName = r.DisplayName,
            Description = r.Description,
            Color = r.GetColor().ToHex(),
            IsDefault = r.IsDefault
        }).ToList();
    }

    public async Task<RoleDto?> GetRoleByNameAsync(string roleName)
    {
        logger.LogDebug("Getting role: {RoleName}", roleName);
        var role = await roleMetadataRepository.GetRoleMetadataAsync(roleName);
        
        if (role == null)
        {
            return null;
        }

        return new RoleDto
        {
            Name = role.Name,
            DisplayName = role.DisplayName,
            Description = role.Description,
            Color = role.GetColor().ToHex(),
            IsDefault = role.IsDefault
        };
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
    {
        logger.LogInformation("Creating role: {RoleName}", request.Name);

        var color = RoleColor.FromHex(request.Color);
        var role = new Shared.Entity.RoleMetadata
        {
            Name = request.Name,
            DisplayName = request.DisplayName,
            Description = request.Description,
            IsDefault = request.IsDefault
        };
        role.SetColor(color);

        await roleMetadataRepository.UpsertRoleMetadataAsync(role);

        return new RoleDto
        {
            Name = role.Name,
            DisplayName = role.DisplayName,
            Description = role.Description,
            Color = role.GetColor().ToHex(),
            IsDefault = role.IsDefault
        };
    }

    public async Task<RoleDto> UpdateRoleAsync(string roleName, UpdateRoleRequest request)
    {
        logger.LogInformation("Updating role: {RoleName}", roleName);

        var existingRole = await roleMetadataRepository.GetRoleMetadataAsync(roleName);
        if (existingRole == null)
        {
            throw new InvalidOperationException($"Role '{roleName}' not found");
        }

        var color = RoleColor.FromHex(request.Color);
        existingRole.DisplayName = request.DisplayName;
        existingRole.Description = request.Description;
        existingRole.IsDefault = request.IsDefault;
        existingRole.SetColor(color);

        await roleMetadataRepository.UpsertRoleMetadataAsync(existingRole);

        return new RoleDto
        {
            Name = existingRole.Name,
            DisplayName = existingRole.DisplayName,
            Description = existingRole.Description,
            Color = existingRole.GetColor().ToHex(),
            IsDefault = existingRole.IsDefault
        };
    }

    public async Task DeleteRoleAsync(string roleName)
    {
        logger.LogInformation("Deleting role: {RoleName}", roleName);

        var role = await roleMetadataRepository.GetRoleMetadataAsync(roleName);
        if (role == null)
        {
            throw new InvalidOperationException($"Role '{roleName}' not found");
        }

        // Note: This only deletes metadata. Casbin policies should be removed separately
        // via RolePermissionService and UserRoleService
        await roleMetadataRepository.DeleteRoleMetadataAsync(roleName);
    }
}

