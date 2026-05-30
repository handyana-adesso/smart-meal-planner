using RecipeBudgetService.DTOs;
using RecipeBudgetService.Entities;

namespace RecipeBudgetService.Common.Mappers;

public static class RecipeMapper
{
    public static RecipeResponse ToResponse(this Recipe recipe) =>
        new(
            recipe.Id, 
            recipe.Name, 
            recipe.Description, 
            recipe.Servings, 
            recipe.Ingredients.Sum(i => i.PricePerUnit * i.Quantity),
            recipe.CreatedAt,
            recipe.Ingredients.ToResponses().ToList());

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
