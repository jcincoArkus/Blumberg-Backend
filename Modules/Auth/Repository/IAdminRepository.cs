using Shared.Dto;
using Shared.Entity;

namespace Modules.Auth.Repository;

/// <summary>
/// Repository interface for Admin entity operations
/// </summary>
public interface IAdminRepository
{
    /// <summary>
    /// Gets an admin by ID
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <returns>Admin entity or null if not found</returns>
    Task<Admin?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets an admin by ID for auth flows (e.g. refresh). Bypasses tenant filter because
    /// the request has no JWT (refresh sends only body), so tenant context is null.
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <returns>Admin entity or null if not found</returns>
    Task<Admin?> GetByIdForAuthAsync(Guid id);

    /// <summary>
    /// Gets an admin by email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>Admin entity or null if not found</returns>
    Task<Admin?> GetByEmailAsync(string email);

    /// <summary>
    /// Checks if an admin with the given email exists
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsAsync(string email);

    /// <summary>
    /// Gets email addresses of admins in the given organization (for alert notifications). Bypasses tenant filter.
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of admin email addresses</returns>
    Task<IReadOnlyList<string>> GetEmailsByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated admins
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated admins and total count</returns>
    Task<(IReadOnlyList<Admin> Items, int TotalCount)> GetPagedAsync(PaginationRequest request);

    /// <summary>
    /// Creates a new admin
    /// </summary>
    /// <param name="admin">Admin entity</param>
    /// <returns>Created admin entity</returns>
    Task<Admin> CreateAsync(Admin admin);

    /// <summary>
    /// Updates an admin
    /// </summary>
    /// <param name="admin">Admin entity</param>
    /// <returns>Updated admin entity</returns>
    Task<Admin> UpdateAsync(Admin admin);

    /// <summary>
    /// Soft deletes an admin by setting the DeletedAt timestamp
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <returns>True if admin was found and deleted, false otherwise</returns>
    Task<bool> SoftDeleteAsync(Guid id);
}
