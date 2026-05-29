namespace RecipeBudgetService.DTOs;

public record RecipeResponse(
    Guid Id,
    string Name,
    string Description,
    int Servings,
    decimal EstimatedCost);
