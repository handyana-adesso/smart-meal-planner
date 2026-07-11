using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Application.Repositories;

public interface IIngredientRepository
{
    Task<Ingredient> CreateAsync(Ingredient ingredient, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
