using RecipeBudgetService.DTOs;

namespace RecipeBudgetService.Services;

public interface IRecipeService
{
    Task<IEnumerable<RecipeResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<RecipeResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<RecipeResponse> CreateAsync(RecipeRequest request, CancellationToken cancellationToken);
    Task<RecipeResponse> UpdateAsync(Guid id, RecipeRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
