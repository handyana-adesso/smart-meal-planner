using RecipeBudgetService.DTOs;
using RecipeBudgetService.Services;

namespace RecipeBudgetService.Endpoints;

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/recipes")
            .WithTags("Recipes")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Gets recipe/s";
                operation.Description = "Endpoints for managing recipes";
                return Task.CompletedTask;
            });

        group.MapPost("/recipes", Create);
        group.MapGet("/recipes", GetAll);
        group.MapGet("/recipes/{id:guid}", GetById);
        group.MapPut("/recipes/{id:guid}", Update);
        group.MapDelete("/recipes/{id:guid}", Delete);
    }

    static async Task<IResult> Create(IRecipeService recipeService, RecipeRequest request, CancellationToken cancellationToken)
    {
        var recipe = await recipeService.CreateAsync(request, cancellationToken);
        return Results.Created($"/api/recipes/{recipe.Id}", recipe);
    }

    static async Task<IResult> GetAll(IRecipeService recipeService, CancellationToken cancellationToken)
    {
        var recipes = await recipeService.GetAllAsync(cancellationToken);
        return Results.Ok(recipes);
    }

    static async Task<IResult> GetById(IRecipeService recipeService, Guid id, CancellationToken cancellationToken)
    {
        var recipe = await recipeService.GetByIdAsync(id, cancellationToken);
        return recipe is null ? Results.NotFound() : Results.Ok(recipe);
    }

    static async Task<IResult> Update(IRecipeService recipeService, Guid id, RecipeRequest request, CancellationToken cancellationToken)
    {
        var recipe = await recipeService.UpdateAsync(id, request, cancellationToken);
        return recipe is null ? Results.NotFound() : Results.Ok(recipe);
    }

    static async Task<IResult> Delete(IRecipeService recipeService, Guid id, CancellationToken cancellationToken)
    {
        var deleted = await recipeService.DeleteAsync(id, cancellationToken);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
}
