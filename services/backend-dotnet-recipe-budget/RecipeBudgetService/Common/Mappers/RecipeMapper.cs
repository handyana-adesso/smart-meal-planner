using RecipeBudgetService.DTOs;
using RecipeBudgetService.Entities;

namespace RecipeBudgetService.Common.Mappers;

public static class RecipeMapper
{
    public static RecipeResponse ToResponse(this Recipe recipe) =>
        new(recipe.Id, recipe.Name, recipe.Description, recipe.Servings, recipe.EstimatedCost);

    public static Recipe ToEntity(this RecipeResponse response, Guid? id = null) =>
        new() 
        { 
            Id = id ?? Guid.Empty, 
            Name = response.Name, 
            Description = response.Description, 
            Servings = response.Servings, 
            EstimatedCost = response.EstimatedCost 
        };
}
