using RecipeBudgetService.DTOs;
using RecipeBudgetService.Entities;

namespace RecipeBudgetService.Common.Mappers;

public static class IngredientMapper
{
    public static IngredientResponse ToResponse(this Ingredient ingredient)
    {
        return new IngredientResponse(
            ingredient.Id,
            ingredient.Name,
            ingredient.Quantity,
            ingredient.Unit);
    }

    public static IEnumerable<IngredientResponse> ToResponses(this IEnumerable<Ingredient> ingredients) 
        => ingredients.Select(ToResponse);

    public static Ingredient ToEntity(this IngredientRequest request, Guid? id = null)
    {
        return new Ingredient
        {
            Id = id ?? Guid.NewGuid(),
            Name = request.Name,
            Quantity = request.Quantity,
            Unit = request.Unit
        };
    }

    public static IEnumerable<Ingredient> ToEntities(this IEnumerable<IngredientRequest> requests)
        => requests.Select(r => ToEntity(r));
}
