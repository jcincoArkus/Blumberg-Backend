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
}
