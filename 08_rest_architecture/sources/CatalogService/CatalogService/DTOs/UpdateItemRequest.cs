using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs;

public class UpdateItemRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
    public decimal Price { get; set; }

    [Required]
    public int CategoryId { get; set; }
}