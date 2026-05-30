namespace RecipeBudgetService.DTOs;

public record RecipeRequest(
    string Name,
    string? Description = null,
    int Servings = 0,
    List<IngredientRequest>? Ingredients = null);
