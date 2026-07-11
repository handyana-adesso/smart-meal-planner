using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Application.Repositories;

public interface IRecipeRepository
{
    Task<IEnumerable<Recipe>> GetAllAsync(CancellationToken cancellationToken);
    Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Recipe> CreateAsync(Recipe recipe, CancellationToken cancellationToken);
    Task<Recipe?> UpdateAsync(Recipe recipe, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
}
