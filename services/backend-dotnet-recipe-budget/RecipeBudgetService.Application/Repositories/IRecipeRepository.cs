using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Application.Repositories;

public interface IRecipeRepository
{
    Task<IEnumerable<Recipe>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<Recipe?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<Recipe> CreateAsync(Recipe recipe, CancellationToken cancellationToken);
    Task<Recipe?> UpdateAsync(Recipe recipe, Guid userId, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, Guid userId, CancellationToken cancellationToken);
}
