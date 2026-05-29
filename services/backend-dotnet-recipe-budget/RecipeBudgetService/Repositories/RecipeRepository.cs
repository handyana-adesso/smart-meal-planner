using RecipeBudgetService.Data;
using RecipeBudgetService.Entities;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RecipeBudgetService.Tests")]

namespace RecipeBudgetService.Repositories;

internal class RecipeRepository(AppDbContext dbContext) : IRecipeRepository
{
    public async Task<Recipe> CreateAsync(Recipe recipe, CancellationToken cancellationToken)
    {
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync(cancellationToken);
        return recipe;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var recipe = dbContext
            .Recipes
            .FirstOrDefault(r => r.Id == id);

        if (recipe is null)
        {
            return false;
        }

        dbContext
            .Recipes
            .Remove(recipe);

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<Recipe>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext
            .Recipes
            .ToListAsync(cancellationToken);
    }

    public async Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext
            .Recipes
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Recipe?> UpdateAsync(Recipe recipe, CancellationToken cancellationToken)
    {
        var existing = dbContext
            .Recipes
            .FirstOrDefault(r => r.Id == recipe.Id);

        if (existing is null)
        {
            return null;
        }

        existing.Name = recipe.Name;
        existing.Description = recipe.Description;
        existing.Servings = recipe.Servings;
        existing.EstimatedCost = recipe.EstimatedCost;
        
        await dbContext
            .SaveChangesAsync(cancellationToken);

        return existing;
    }
}
