using RecipeBudgetService.Common.Filters;
using RecipeBudgetService.DTOs;
using RecipeBudgetService.Services;

namespace RecipeBudgetService.Endpoints;

public static class IngredientEndpoints
{
    public static void MapIngredientEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/recipes/{recipeId:guid}/ingredients")
            .WithTags("Ingredients")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Gets ingredients";
                operation.Description = "Endpoints for managing ingredients";
                return Task.CompletedTask;
            });

        group.MapPost("/", CreateAsync)
            .AddEndpointFilter<ValidationFilter<IngredientRequest>>();
        group.MapDelete("/{ingredientId:guid}", DeleteAsync);
    }

    static async Task<IResult> CreateAsync(
        Guid recipeId, 
        IngredientRequest request, 
        IIngredientService ingredientService, 
        CancellationToken cancellationToken)
    {
        var result = await ingredientService.CreateAsync(recipeId, request, cancellationToken);
        return Results.Created($"/recipes/{recipeId}/ingredients/{result.Id}", result);
    }

    static async Task<IResult> DeleteAsync(
        Guid recipeId,
        Guid ingredientId,
        IIngredientService ingredientService,
        CancellationToken cancellationToken)
    {
        await ingredientService.DeleteAsync(recipeId, ingredientId, cancellationToken);
        return Results.NoContent();
    }
}
