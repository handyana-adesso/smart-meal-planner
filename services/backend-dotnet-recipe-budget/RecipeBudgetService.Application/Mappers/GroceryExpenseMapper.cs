using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Application.Mappers;

public static class GroceryExpenseMapper
{
    public static GroceryExpenseResponse ToResponse(this GroceryExpense expense)
        => new(
            expense.Id,
            expense.Description,
            expense.Amount,
            expense.Category,
            expense.Category.ToString(),
            expense.Date,
            expense.CreatedAt,
            expense.RecipeId,
            expense.Recipe?.Name
        );

    public static IEnumerable<GroceryExpenseResponse> ToResponses(this IEnumerable<GroceryExpense> expenses)
        => expenses.Select(ToResponse);

    public static GroceryExpense ToEntity(this GroceryExpenseRequest request, Guid? id = null)
        => new()
        {
            Id = id ?? Guid.Empty,
            Description = request.Description,
            Amount = request.Amount,
            Category = request.Category,
            Date = request.Date ?? DateTime.UtcNow,
            RecipeId = request.RecipeId
        };
}
