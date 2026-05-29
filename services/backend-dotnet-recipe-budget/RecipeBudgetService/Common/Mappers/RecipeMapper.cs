using RecipeBudgetService.DTOs;
using RecipeBudgetService.Entities;

namespace RecipeBudgetService.Common.Mappers;

public static class RecipeMapper
{
    public static RecipeResponse ToResponse(this Recipe recipe) =>
        new(recipe.Id, recipe.Name, recipe.Description, recipe.Servings, recipe.EstimatedCost);

    public static Recipe ToEntity(this RecipeRequest request, Guid? id = null) =>
        new() 
        { 
            Id = id ?? Guid.NewGuid(), 
            Name = request.Name, 
            Description = request.Description ?? string.Empty, 
            Servings = request.Servings, 
            EstimatedCost = request.EstimatedCost 
        };
}
