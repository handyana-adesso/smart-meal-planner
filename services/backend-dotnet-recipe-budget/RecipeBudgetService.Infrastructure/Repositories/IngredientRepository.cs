using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Application.Repositories;
using RecipeBudgetService.Infrastructure.Data;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RecipeBudgetService.Tests")]

namespace RecipeBudgetService.Infrastructure.Repositories;

public class IngredientRepository(AppDbContext dbContext) : IIngredientRepository
{
    public async Task<Ingredient> CreateAsync(Ingredient ingredient, CancellationToken cancellationToken)
    {
        dbContext.Ingredients.Add(ingredient);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ingredient;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var ingredient = dbContext
            .Ingredients
            .FirstOrDefault(i => i.Id == id);

        if (ingredient is null)
        {
            return false;
        }

        dbContext.Ingredients.Remove(ingredient);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
