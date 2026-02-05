using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;

namespace Modules.Auth.Repository;

/// <summary>
/// Repository implementation for Admin entity operations
/// </summary>
public class AdminRepository(ApplicationDbContext context, ILogger<AdminRepository> logger) : IAdminRepository
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<Admin?> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Querying admin by ID: {Id}", id);

        var admin = await context.Admins
            .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null);

        if (admin != null)
            logger.LogInformation("Found admin {Id}", id);
        else
            logger.LogDebug("Admin not found with ID: {Id}", id);

        return admin;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Admin?> GetByEmailAsync(string email)
    {
        logger.LogDebug("Querying admin by email: {Email}", email);

        // Bypass tenant filter for login: find admin by email across organizations
        var admin = await context.Admins
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Email == email && a.DeletedAt == null);

        if (admin != null)
            logger.LogInformation("Found admin with email: {Email}", email);
        else
            logger.LogDebug("Admin not found with email: {Email}", email);

        return admin;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<bool> ExistsAsync(string email)
    {
        logger.LogDebug("Checking if admin exists with email: {Email}", email);

        var exists = await context.Admins
            .AnyAsync(a => a.Email == email && a.DeletedAt == null);

        logger.LogDebug("Admin exists check for {Email}: {Exists}", email, exists);

        return exists;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<List<Admin>> GetAllAsync()
    {
        logger.LogDebug("Querying all admins");

        var admins = await context.Admins
            .Where(a => a.DeletedAt == null)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} admins from database", admins.Count);

        return admins;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Admin> CreateAsync(Admin admin)
    {
        logger.LogDebug("Creating admin in database: {Email}", admin.Email);

        context.Admins.Add(admin);
        await context.SaveChangesAsync();

        logger.LogInformation("Admin created in database with ID: {Id}", admin.Id);

        return admin;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Admin> UpdateAsync(Admin admin)
    {
        logger.LogDebug("Updating admin in database: {Id}", admin.Id);

        context.Admins.Update(admin);
        await context.SaveChangesAsync();

        logger.LogInformation("Admin {Id} updated in database", admin.Id);

        return admin;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Admin> DeleteAsync(Admin admin)
    {
        logger.LogDebug("Soft deleting admin in database: {Id}", admin.Id);

        // Soft delete: set DeletedAt timestamp
        admin.DeletedAt = DateTime.UtcNow;
        admin.UpdatedAt = DateTime.UtcNow;

        context.Admins.Update(admin);
        await context.SaveChangesAsync();

        logger.LogInformation("Admin {Id} soft deleted in database", admin.Id);

        return admin;
    }
}

