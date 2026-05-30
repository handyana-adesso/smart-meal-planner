using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace RecipeBudgetService.Entities;

public class Recipe
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    [Range(0, int.MaxValue, ErrorMessage = "Value must be greater than or equal to zero.")]
    public int Servings { get; set; }
    [Precision(18, 2)]
    public decimal EstimatedCost { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
}
