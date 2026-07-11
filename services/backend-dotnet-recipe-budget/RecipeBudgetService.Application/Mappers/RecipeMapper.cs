using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Application.Mappers;

public static class RecipeMapper
{
    public static RecipeResponse ToResponse(this Recipe recipe) =>
        new(
            recipe.Id, 
            recipe.Name, 
            recipe.Description, 
            recipe.Servings, 
            recipe.Ingredients.ToResponses());

    public static Recipe ToEntity(this RecipeRequest request, Guid? id = null) =>
        new() 
        { 
            Id = id ?? Guid.NewGuid(), 
            Name = request.Name, 
            Description = request.Description ?? string.Empty, 
            Servings = request.Servings, 
            Ingredients = request.Ingredients?
                .ToEntities()
                .ToList() ?? new List<Ingredient>()
        };
}
