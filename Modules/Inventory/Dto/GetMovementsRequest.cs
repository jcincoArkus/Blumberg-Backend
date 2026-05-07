using Shared.Dto;

namespace Modules.Inventory.Dto;

/// <summary>Filter / pagination parameters for movements list</summary>
public class GetMovementsRequest : PaginationRequest
{
    /// <summary>Filter by movement type</summary>
    public string? Type { get; set; }

    /// <summary>Filter by product</summary>
    public Guid? ProductId { get; set; }

    /// <summary>Filter by source site</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Filter by lot code</summary>
    public string? LotCode { get; set; }

    /// <summary>Only return movements at or after this date</summary>
    public DateTime? From { get; set; }

    /// <summary>Only return movements at or before this date</summary>
    public DateTime? To { get; set; }
}
