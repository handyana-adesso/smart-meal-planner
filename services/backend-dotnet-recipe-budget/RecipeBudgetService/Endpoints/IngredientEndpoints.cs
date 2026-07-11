using Microsoft.AspNetCore.Mvc;
using RecipeBudgetService.Filters;
using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Application.Services;

namespace RecipeBudgetService.Endpoints;

public static class IngredientEndpoints
{
    public static void MapIngredientEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/recipes/{recipeId:guid}/ingredients")
            .WithTags("Ingredients");

        group.MapPost("/", CreateAsync)
            .WithName("CreateIngredient")
            .WithSummary("Create an ingredient")
            .WithDescription("Creates a new ingredient.")
            .Produces<RecipeResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .AddEndpointFilter<ValidationFilter<IngredientRequest>>();
        group.MapDelete("/{ingredientId:guid}", DeleteAsync)
            .WithName("DeleteIngredient")
            .WithSummary("Delete an ingredient")
            .WithDescription("Deletes an ingredient by recipe id and its id.")
            .Produces(StatusCodes.Status204NoContent);
    }

    static async Task<IResult> CreateAsync(
        Guid recipeId, 
        IngredientRequest request,
        [FromServices] IIngredientService ingredientService, 
        CancellationToken cancellationToken)
    {
        var result = await ingredientService.CreateAsync(recipeId, request, cancellationToken);
        return Results.Created($"/api/recipes/{recipeId}/ingredients/{result.Id}", result);
    }

    static async Task<IResult> DeleteAsync(
        Guid recipeId,
        Guid ingredientId,
        [FromServices] IIngredientService ingredientService,
        CancellationToken cancellationToken)
    {
        await ingredientService.DeleteAsync(recipeId, ingredientId, cancellationToken);
        return Results.NoContent();
    }
}
