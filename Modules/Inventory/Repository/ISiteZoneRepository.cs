using Shared.Dto;
using Shared.Entity;

namespace Modules.Inventory.Repository;

public interface ISiteZoneRepository
{
    Task<(IReadOnlyList<SiteZone> Items, int TotalCount)> GetPagedAsync(PaginationRequest request, Guid? siteId);
    Task<SiteZone?> GetByIdAsync(Guid id);
    Task<SiteZone> CreateAsync(SiteZone entity);
    Task<SiteZone> UpdateAsync(SiteZone entity);
    Task<bool> DeleteAsync(Guid id);
}
