using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Admins.Dto;
using Modules.Auth.Repository;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Admins.Service;

/// <summary>
/// Service implementation for admin operations
/// </summary>
public class AdminService(IAdminRepository adminRepository, ILogger<AdminService> logger) : IAdminService
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<Admin> Items, int TotalCount)> GetAllAsync(PaginationRequest request)
    {
        logger.LogDebug("Getting admins page {Page}, pageSize {PageSize}", request.Page, request.PageSize);

        var result = await adminRepository.GetPagedAsync(request);

        logger.LogInformation("Retrieved {Count} admins (total: {TotalCount})", result.Items.Count, result.TotalCount);

        return result;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Admin> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Getting admin by ID: {Id}", id);

        var admin = await adminRepository.GetByIdAsync(id);

        if (admin == null)
        {
            logger.LogWarning("Admin not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Admin with ID {id} was not found.");
        }

        logger.LogInformation("Retrieved admin {Id}", id);

        return admin;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Admin> CreateAsync(AdminRequest request)
    {
        logger.LogDebug("Creating new admin with email: {Email}", request.Email);

        // Check if email already exists
        var emailExists = await adminRepository.ExistsAsync(request.Email);
        if (emailExists)
        {
            logger.LogWarning("Failed to create admin: email already exists: {Email}", request.Email);
            throw new InvalidOperationException($"Admin with email {request.Email} already exists.");
        }

        // Create new admin entity
        var newAdmin = new Admin
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        // Save to database
        var createdAdmin = await adminRepository.CreateAsync(newAdmin);

        logger.LogInformation("Admin created successfully with ID: {Id}, Email: {Email}", createdAdmin.Id, createdAdmin.Email);

        return createdAdmin;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Admin> UpdateAsync(Guid id, AdminRequest request)
    {
        logger.LogDebug("Updating admin {Id}", id);

        var admin = await adminRepository.GetByIdAsync(id);
        if (admin == null)
        {
            logger.LogWarning("Admin not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Admin with ID {id} was not found.");
        }

        // Update admin entity
        admin.Email = request.Email;
        admin.FirstName = request.FirstName;
        admin.LastName = request.LastName;
        admin.UpdatedAt = DateTime.UtcNow;

        // Save to database
        var updatedAdmin = await adminRepository.UpdateAsync(admin);

        logger.LogInformation("Admin {Id} updated successfully", id);

        return updatedAdmin;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task DeleteAsync(Guid id)
    {
        logger.LogDebug("Deleting admin {Id}", id);

        var deleted = await adminRepository.SoftDeleteAsync(id);

        if (!deleted)
        {
            logger.LogWarning("Admin not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Admin with ID {id} was not found.");
        }

        logger.LogInformation("Admin {Id} deleted successfully", id);
    }
}
