
using RecipeBudgetService.Common.Exceptions;
using RecipeBudgetService.Common.Extensions;
using RecipeBudgetService.Common.Mappers;
using RecipeBudgetService.DTOs;
using RecipeBudgetService.Repositories;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RecipeBudgetService.Tests")]

namespace RecipeBudgetService.Services;

internal class IngredientService(
    IRecipeRepository recipeRepository,
    IIngredientRepository ingredientRepository) 
    : IIngredientService
{
    public async Task<IngredientResponse> CreateAsync(Guid recipeId, IngredientRequest request, CancellationToken cancellationToken)
    {
        GuardExtensions.ThrowIfGuidEmpty(recipeId);
        ArgumentNullException.ThrowIfNull(request);

        var recipe = recipeRepository.GetByIdAsync(recipeId, cancellationToken).Result;
        if (recipe is null)
        {
            throw new NotFoundException($"Recipe with id {recipeId} was not found.");
        }

        var ingredient = request.ToEntity();
        ingredient.RecipeId = recipeId;

        var added = await ingredientRepository.CreateAsync(ingredient, cancellationToken);
        return added.ToResponse();
    }

    public async Task DeleteAsync(Guid recipeId, Guid ingredientId, CancellationToken cancellationToken)
    {
        GuardExtensions.ThrowIfGuidEmpty(recipeId);
        GuardExtensions.ThrowIfGuidEmpty(ingredientId);

        var recipe = await recipeRepository.GetByIdAsync(recipeId, cancellationToken);
        if (recipe is null)
        {
            throw new NotFoundException($"Recipe with id {recipeId} was not found.");
        }

        var removed = await ingredientRepository.DeleteAsync(ingredientId, cancellationToken);
        if (!removed)
        {
            throw new NotFoundException($"Ingredient with id {ingredientId} was not found.");
        }
    }
}
