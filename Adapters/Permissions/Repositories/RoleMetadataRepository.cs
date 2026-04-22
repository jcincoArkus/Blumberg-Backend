using System.Text.Json;
using Adapters.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;
using Shared.ValueObjects;

namespace Adapters.Permissions.Repositories;

/// <summary>
/// Repository implementation for role metadata operations
/// </summary>
public class RoleMetadataRepository(ApplicationDbContext context, ILogger<RoleMetadataRepository> logger) 
    : IRoleMetadataRepository
{
    private const string ConfigFilePath = "Adapters/Permissions/casbin_roles.json";

    public async Task<RoleMetadata?> GetRoleMetadataAsync(string roleName)
    {
        logger.LogDebug("Getting role metadata for {RoleName}", roleName);
        return await context.RoleMetadata
            .FirstOrDefaultAsync(r => r.Name == roleName);
    }

    public async Task<List<RoleMetadata>> GetAllRoleMetadataAsync()
    {
        logger.LogDebug("Getting all role metadata");
        return await context.RoleMetadata.ToListAsync();
    }

    public async Task UpsertRoleMetadataAsync(RoleMetadata metadata)
    {
        logger.LogDebug("Upserting role metadata for {RoleName}", metadata.Name);

        var existing = await context.RoleMetadata
            .FirstOrDefaultAsync(r => r.Name == metadata.Name);

        if (existing != null)
        {
            // Update existing
            existing.DisplayName = metadata.DisplayName;
            existing.Description = metadata.Description;
            existing.ColorR = metadata.ColorR;
            existing.ColorG = metadata.ColorG;
            existing.ColorB = metadata.ColorB;
            existing.IsDefault = metadata.IsDefault;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Create new
            await context.RoleMetadata.AddAsync(metadata);
        }

        await context.SaveChangesAsync();
    }

    public async Task LoadRolesFromConfigAsync()
    {
        logger.LogInformation("Loading role metadata from {ConfigFile}", ConfigFilePath);

        if (!File.Exists(ConfigFilePath))
        {
            logger.LogWarning("Config file not found: {ConfigFile}", ConfigFilePath);
            return;
        }

        var json = await File.ReadAllTextAsync(ConfigFilePath);
        var config = JsonSerializer.Deserialize<RolesConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.Roles == null || config.Roles.Count == 0)
        {
            logger.LogWarning("No roles found in config file");
            return;
        }

        logger.LogInformation("Found {Count} roles in config", config.Roles.Count);

        foreach (var roleConfig in config.Roles)
        {
            var metadata = new RoleMetadata
            {
                Name = roleConfig.Name,
                DisplayName = roleConfig.DisplayName,
                Description = roleConfig.Description,
                ColorR = roleConfig.Color.R,
                ColorG = roleConfig.Color.G,
                ColorB = roleConfig.Color.B,
                IsDefault = roleConfig.IsDefault
            };

            await UpsertRoleMetadataAsync(metadata);
            logger.LogInformation("Loaded role: {RoleName} ({DisplayName})", metadata.Name, metadata.DisplayName);
        }

        logger.LogInformation("Successfully loaded {Count} roles from config", config.Roles.Count);
    }

    public async Task<RoleMetadata?> GetDefaultRoleAsync()
    {
        logger.LogDebug("Getting default role");
        return await context.RoleMetadata
            .FirstOrDefaultAsync(r => r.IsDefault);
    }

    public async Task DeleteRoleMetadataAsync(string roleName)
    {
        logger.LogInformation("Deleting role metadata: {RoleName}", roleName);
        var role = await context.RoleMetadata
            .FirstOrDefaultAsync(r => r.Name == roleName);

        if (role != null)
        {
            context.RoleMetadata.Remove(role);
            await context.SaveChangesAsync();
            logger.LogInformation("Role metadata deleted: {RoleName}", roleName);
        }
        else
        {
            logger.LogWarning("Role metadata not found: {RoleName}", roleName);
        }
    }

    // Internal classes for JSON deserialization
    private class RolesConfig
    {
        public List<RoleConfig> Roles { get; set; } = new();
    }

    private class RoleConfig
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ColorConfig Color { get; set; } = new();
        public bool IsDefault { get; set; }
    }

    private class ColorConfig
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
    }
}

