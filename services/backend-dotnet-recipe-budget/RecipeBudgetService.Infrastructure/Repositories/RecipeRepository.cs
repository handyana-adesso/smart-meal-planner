using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Application.Repositories;
using RecipeBudgetService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace RecipeBudgetService.Infrastructure.Repositories;

public class RecipeRepository(AppDbContext dbContext) : IRecipeRepository
{
    public async Task<Recipe> CreateAsync(Recipe recipe, CancellationToken cancellationToken)
    {
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync(cancellationToken);
        return recipe;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var recipe = dbContext
            .Recipes
            .FirstOrDefault(r => r.Id == id && r.UserId == userId);

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

    public async Task<IEnumerable<Recipe>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext
            .Recipes
            .Include(r => r.Ingredients)
            .Where(r => r.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Recipe?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext
            .Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);
    }

    public async Task<Recipe?> UpdateAsync(Recipe recipe, Guid userId, CancellationToken cancellationToken)
    {
        var existing = dbContext
            .Recipes
            .FirstOrDefault(r => r.Id == recipe.Id && r.UserId == userId);

        if (existing is null)
        {
            return null;
        }

        existing.Name = recipe.Name;
        existing.Description = recipe.Description;
        existing.Servings = recipe.Servings;

        // Replace Ingredients
        existing.Ingredients.Clear();
        foreach (var ingredient in recipe.Ingredients)
        {
            existing.Ingredients.Add(ingredient);
        }

        await dbContext
            .SaveChangesAsync(cancellationToken);

        return existing;
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext
            .Recipes
            .AnyAsync(r => r.Name.ToLower() == name.ToLower() && r.UserId == userId, cancellationToken);
    }
}
