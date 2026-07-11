using RecipeBudgetService.Application.DTOs;

namespace RecipeBudgetService.Application.Services;

public interface IIngredientService
{
    Task<IngredientResponse> CreateAsync(Guid recipeId, IngredientRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid recipeId, Guid ingredientId, CancellationToken cancellationToken);
}
