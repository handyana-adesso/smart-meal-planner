using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Application.DTOs;

public record GroceryExpenseRequest(
    string Description,
    decimal Amount,
    ExpenseCategory Category,
    DateTime? Date = null,
    Guid? RecipeId = null
);
