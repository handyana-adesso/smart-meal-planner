using RecipeBudgetService.Domain.Exceptions;
using RecipeBudgetService.Application.Extensions;
using RecipeBudgetService.Application.Mappers;
using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Application.Repositories;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RecipeBudgetService.Tests")]

namespace RecipeBudgetService.Application.Services;

public class IngredientService(
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
