using Microsoft.AspNetCore.Mvc;
using RecipeBudgetService.Common.Filters;
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

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<RecipeRequest>>();
        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<RecipeRequest>>();
        group.MapDelete("/{id:guid}", Delete);
    }

    static async Task<IResult> Create([FromServices] IRecipeService recipeService, RecipeRequest request, CancellationToken cancellationToken)
    {
        var recipe = await recipeService.CreateAsync(request, cancellationToken);
        return Results.Created($"/api/recipes/{recipe.Id}", recipe);
    }

    static async Task<IResult> GetAll([FromServices] IRecipeService recipeService, CancellationToken cancellationToken)
    {
        var recipes = await recipeService.GetAllAsync(cancellationToken);
        return Results.Ok(recipes);
    }

    static async Task<IResult> GetById([FromServices] IRecipeService recipeService, Guid id, CancellationToken cancellationToken)
    {
        var recipe = await recipeService.GetByIdAsync(id, cancellationToken);
        return Results.Ok(recipe);
    }

    static async Task<IResult> Update([FromServices] IRecipeService recipeService, Guid id, RecipeRequest request, CancellationToken cancellationToken)
    {
        var recipe = await recipeService.UpdateAsync(id, request, cancellationToken);
        return Results.Ok(recipe);
    }

    static async Task<IResult> Delete([FromServices] IRecipeService recipeService, Guid id, CancellationToken cancellationToken)
    {
        await recipeService.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }
}
