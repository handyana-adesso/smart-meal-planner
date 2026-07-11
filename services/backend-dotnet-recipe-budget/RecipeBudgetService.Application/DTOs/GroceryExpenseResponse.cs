using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Application.DTOs;

public record GroceryExpenseResponse(
    Guid Id,
    string Description,
    decimal Amount,
    ExpenseCategory Category,
    string CategoryName,
    DateTime Date,
    DateTime CreatedAt,
    Guid? RecipeId,
    string? RecipeName
);
