using Adapters.Telemetry;
using Microsoft.Extensions.Logging;
using Modules.Sites.Dto;
using Modules.Sites.Repository;
using Shared.Dto;
using Shared.Entity;

namespace Modules.Sites.Service;

/// <summary>
/// Service implementation for site operations
/// </summary>
public class SiteService(ISiteRepository siteRepository, ILogger<SiteService> logger) : ISiteService
{
    /// <inheritdoc />
    [Span]
    public virtual async Task<(IReadOnlyList<Site> Items, int TotalCount)> GetAllAsync(PaginationRequest request)
    {
        logger.LogDebug("Getting sites page {Page}, pageSize {PageSize}", request.Page, request.PageSize);

        var result = await siteRepository.GetPagedAsync(request);

        logger.LogInformation("Retrieved {Count} sites (total: {TotalCount})", result.Items.Count, result.TotalCount);

        return result;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task<Site> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Getting site by ID: {Id}", id);

        var site = await siteRepository.GetByIdAsync(id);

        if (site == null)
        {
            logger.LogWarning("Site not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Site with ID {id} was not found");
        }

        logger.LogInformation("Retrieved site {Id}", id);

        return site;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Site> CreateAsync(SiteRequest request)
    {
        logger.LogDebug("Creating new site: {Name}", request.Name);

        var site = new Site
        {
            Name = request.Name,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country
        };

        var createdSite = await siteRepository.CreateAsync(site);

        logger.LogInformation("Site created successfully with ID: {Id}", createdSite.Id);

        return createdSite;
    }

    /// <inheritdoc />
    [Span(IncludeArguments = true)]
    public virtual async Task<Site> UpdateAsync(Guid id, SiteRequest request)
    {
        logger.LogDebug("Updating site {Id}", id);

        var site = await siteRepository.GetByIdAsync(id);

        if (site == null)
        {
            logger.LogWarning("Site not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Site with ID {id} was not found");
        }

        site.Name = request.Name;
        site.Address = request.Address;
        site.City = request.City;
        site.State = request.State;
        site.PostalCode = request.PostalCode;
        site.Country = request.Country;

        var updatedSite = await siteRepository.UpdateAsync(site);

        logger.LogInformation("Site {Id} updated successfully", id);

        return updatedSite;
    }

    /// <inheritdoc />
    [Span]
    public virtual async Task DeleteAsync(Guid id)
    {
        logger.LogDebug("Deleting site {Id}", id);

        var deleted = await siteRepository.SoftDeleteAsync(id);

        if (!deleted)
        {
            logger.LogWarning("Site not found with ID: {Id}", id);
            throw new KeyNotFoundException($"Site with ID {id} was not found");
        }

        logger.LogInformation("Site {Id} deleted successfully", id);
    }
}
