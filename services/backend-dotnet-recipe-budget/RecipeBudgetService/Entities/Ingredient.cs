using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RecipeBudgetService.Entities;

public class Ingredient
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Precision(18, 2)]
    public decimal Quantity { get; set; }
    [Required]
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;

    // Foreign key to Recipe
    public Guid RecipeId { get; set; }

    // Navigation to Recipe
    public Recipe Recipe { get; set; } = null!;
}
