using System.ComponentModel.DataAnnotations;

namespace Modules.Inventory.Dto;

/// <summary>Create / update a supplier</summary>
public class SupplierRequest
{
    /// <summary>Supplier company name</summary>
    /// <example>Frutería Morales</example>
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Geographic latitude (-90 to 90)</summary>
    /// <example>19.4326</example>
    [Range(-90, 90)]
    public decimal? Latitude { get; set; }

    /// <summary>Geographic longitude (-180 to 180)</summary>
    /// <example>-99.1332</example>
    [Range(-180, 180)]
    public decimal? Longitude { get; set; }
}
