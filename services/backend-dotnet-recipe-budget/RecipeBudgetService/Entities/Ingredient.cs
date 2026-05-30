using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RecipeBudgetService.Entities;

public class Ingredient
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue)]
    [Precision(18, 2)]
    public decimal Quantity { get; set; }
    [Required]
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;
    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative.")]
    [Precision(18, 2)]
    public decimal PricePerUnit { get; set; }

    // Foreign key to Recipe
    public Guid RecipeId { get; set; }

    // Navigation to Recipe
    public Recipe Recipe { get; set; } = null!;
}
