using Adapters.Permissions.Repositories;
using Microsoft.Extensions.Logging;

namespace Adapters.Permissions;

/// <summary>
/// Bootstrap service for initializing RBAC policies and role metadata
/// </summary>
public class PermissionBootstrap(
    IRbacRepository rbacRepository,
    IRoleMetadataRepository roleMetadataRepository,
    ILogger<PermissionBootstrap> logger)
{
    /// <summary>
    /// Bootstraps the complete RBAC system: policies and role metadata
    /// </summary>
    public async Task BootstrapAsync()
    {
        logger.LogInformation("Starting RBAC bootstrap");

        await BootstrapPoliciesAsync();
        await BootstrapRoleMetadataAsync();

        logger.LogInformation("RBAC bootstrap completed successfully");
    }

    /// <summary>
    /// Syncs RBAC policies from CSV with database
    /// This function performs full synchronization:
    /// - Adds policies that are in CSV but not in database
    /// - Removes policies that are in database but not in CSV
    /// </summary>
    private async Task BootstrapPoliciesAsync()
    {
        logger.LogInformation("Bootstrapping RBAC policies from CSV");

        // Get existing policies from database
        var existingPolicies = await rbacRepository.GetAllPoliciesAsync();
        var existingPolicySet = existingPolicies
            .Select(p => $"{p[0]}:{p[1]}:{p[2]}")
            .ToHashSet();

        logger.LogInformation("Found {Count} existing policies in database", existingPolicies.Count);

        // Read policies from CSV
        var csvPolicies = await ReadPoliciesFromCsvAsync();
        var csvPolicySet = csvPolicies
            .Select(p => $"{p.Role}:{p.Resource}:{p.Action}")
            .ToHashSet();

        logger.LogInformation("Found {Count} policies in CSV", csvPolicies.Count);

        // Add missing policies (in CSV but not in DB)
        var addedCount = 0;
        foreach (var policy in csvPolicies)
        {
            var key = $"{policy.Role}:{policy.Resource}:{policy.Action}";
            if (!existingPolicySet.Contains(key))
            {
                var added = await rbacRepository.AddPolicyAsync(policy.Role, policy.Resource, policy.Action);
                if (added)
                {
                    addedCount++;
                    logger.LogDebug("Added policy: {Role} -> {Resource} -> {Action}", 
                        policy.Role, policy.Resource, policy.Action);
                }
            }
        }

        // Remove obsolete policies (in DB but not in CSV)
        var removedCount = 0;
        foreach (var policy in existingPolicies)
        {
            if (policy.Count >= 3)
            {
                var key = $"{policy[0]}:{policy[1]}:{policy[2]}";
                if (!csvPolicySet.Contains(key))
                {
                    var removed = await rbacRepository.RemovePolicyAsync(policy[0], policy[1], policy[2]);
                    if (removed)
                    {
                        removedCount++;
                        logger.LogDebug("Removed policy: {Role} -> {Resource} -> {Action}", 
                            policy[0], policy[1], policy[2]);
                    }
                }
            }
        }

        // Get final count
        var finalPolicies = await rbacRepository.GetAllPoliciesAsync();

        if (addedCount > 0 || removedCount > 0)
        {
            logger.LogInformation("Policy sync complete - Added: {Added}, Removed: {Removed}, Total: {Total}",
                addedCount, removedCount, finalPolicies.Count);
        }
        else
        {
            logger.LogInformation("All policies up to date ({Count} policies)", finalPolicies.Count);
        }
    }

    /// <summary>
    /// Loads role metadata from JSON configuration file
    /// </summary>
    private async Task BootstrapRoleMetadataAsync()
    {
        logger.LogInformation("Bootstrapping role metadata from JSON config");

        await roleMetadataRepository.LoadRolesFromConfigAsync();

        var allMetadata = await roleMetadataRepository.GetAllRoleMetadataAsync();
        logger.LogInformation("Successfully loaded metadata for {Count} roles", allMetadata.Count);
    }

    /// <summary>
    /// Reads policies from the CSV file
    /// </summary>
    private async Task<List<PolicyEntry>> ReadPoliciesFromCsvAsync()
    {
        const string csvPath = "Adapters/Permissions/casbin_policy.csv";
        var policies = new List<PolicyEntry>();

        if (!File.Exists(csvPath))
        {
            logger.LogWarning("Policy CSV file not found: {Path}", csvPath);
            return policies;
        }

        var lines = await File.ReadAllLinesAsync(csvPath);
        var lineNumber = 0;

        foreach (var line in lines)
        {
            lineNumber++;
            var trimmed = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                continue;

            // Parse CSV: p, role, resource, action
            var parts = trimmed.Split(',');
            if (parts.Length < 4)
            {
                logger.LogWarning("Invalid policy format at line {LineNumber}: {Line}", lineNumber, line);
                continue;
            }

            var policyType = parts[0].Trim();
            if (policyType != "p")
                continue;

            policies.Add(new PolicyEntry(
                parts[1].Trim(),
                parts[2].Trim(),
                parts[3].Trim()
            ));
        }

        return policies;
    }

    private record PolicyEntry(string Role, string Resource, string Action);
}

