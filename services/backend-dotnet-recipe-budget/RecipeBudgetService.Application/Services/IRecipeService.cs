using RecipeBudgetService.Application.DTOs;

namespace RecipeBudgetService.Application.Services;

public interface IRecipeService
{
    Task<IEnumerable<RecipeResponse>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<RecipeResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<RecipeResponse> CreateAsync(RecipeRequest request, Guid userId, CancellationToken cancellationToken);
    Task<RecipeResponse> UpdateAsync(Guid id, RecipeRequest request, Guid userId, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}
