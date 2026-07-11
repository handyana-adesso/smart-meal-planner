namespace RecipeBudgetService.Application.DTOs;

public record RecipeResponse(Guid Id, string Name, string? Description, int Servings, IEnumerable<IngredientResponse>? Ingredients);
