using Adapters.Database;
using Adapters.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Dto;
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
    public virtual async Task<Admin?> GetByIdForAuthAsync(Guid id)
    {
        logger.LogDebug("Querying admin by ID for auth: {Id}", id);
        // Bypass tenant filter for refresh: request has no JWT so tenant context is null; id comes from validated refresh token.
        var admin = await context.Admins
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null);
        if (admin != null)
            logger.LogInformation("Found admin for auth: {Id}", id);
        else
            logger.LogDebug("Admin not found for auth with ID: {Id}", id);
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
    public virtual async Task<IReadOnlyList<string>> GetEmailsByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await context.Admins
            .IgnoreQueryFilters()
            .Where(a => a.OrganizationId == organizationId && a.DeletedAt == null)
            .Select(a => a.Email)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<Admin> Items, int TotalCount)> GetPagedAsync(PaginationRequest request)
    {
        logger.LogDebug("Querying admins page {Page}, pageSize {PageSize}, search '{Search}'", request.Page, request.PageSize, request.Search);

        var query = context.Admins
            .Where(a => a.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(a =>
                a.Email.ToLower().Contains(search) ||
                a.FirstName.ToLower().Contains(search) ||
                a.LastName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        logger.LogInformation("Retrieved {Count} admins from database (total: {TotalCount})", items.Count, totalCount);

        return (items, totalCount);
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Admin> CreateAsync(Admin admin)
    {
        logger.LogDebug("Creating admin in database: {Email}", admin.Email);

        admin.Id = Guid.NewGuid();
        admin.CreatedAt = DateTime.UtcNow;
        admin.UpdatedAt = null;
        admin.DeletedAt = null;

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
    public virtual async Task<bool> SoftDeleteAsync(Guid id)
    {
        logger.LogDebug("Soft deleting admin in database: {Id}", id);

        var admin = await GetByIdAsync(id);
        if (admin == null)
        {
            logger.LogWarning("Admin not found for deletion: {Id}", id);
            return false;
        }

        admin.DeletedAt = DateTime.UtcNow;
        admin.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Admin {Id} soft deleted in database", id);

        return true;
    }
}
