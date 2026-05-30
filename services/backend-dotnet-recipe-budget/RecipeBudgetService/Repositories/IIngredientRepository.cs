using RecipeBudgetService.Entities;

namespace RecipeBudgetService.Repositories;

public interface IIngredientRepository
{
    Task<Ingredient> CreateAsync(Ingredient ingredient, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
