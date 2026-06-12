using RecipeBudgetService.Entities;

namespace RecipeBudgetService.DTOs;

public record GroceryExpenseRequest(
    string Description,
    decimal Amount,
    ExpenseCategory Category,
    DateTime? Date = null,
    Guid? RecipeId = null
);
