using System.ComponentModel.DataAnnotations;

namespace RecipeBudgetService.Domain.Entities;

public class Recipe
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Value must be at least 1.")]
    public int Servings { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Ingredient> Ingredients { get; set; } = [];
    public ICollection<GroceryExpense> GroceryExpenses { get; set; } = [];
}
