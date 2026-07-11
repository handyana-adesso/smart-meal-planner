using System.ComponentModel.DataAnnotations;

namespace RecipeBudgetService.Domain.Entities;

public class GroceryExpense
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    public ExpenseCategory Category { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? RecipeId { get; set; }
    public Recipe? Recipe { get; set; }
}
