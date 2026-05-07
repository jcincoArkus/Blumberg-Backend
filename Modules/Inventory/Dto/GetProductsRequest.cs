using Shared.Dto;

namespace Modules.Inventory.Dto;

/// <summary>Filter / pagination parameters for products list</summary>
public class GetProductsRequest : PaginationRequest
{
    /// <summary>Filter by category</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Filter by unit ("kg" | "unit" | "box")</summary>
    public string? Unit { get; set; }
}
