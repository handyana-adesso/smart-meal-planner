using Microsoft.AspNetCore.Mvc;
using RecipeBudgetService.Filters;
using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Application.Services;

namespace RecipeBudgetService.Endpoints;

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/recipes")
            .WithTags("Recipes");

        group.MapPost("/", Create)
            .WithName("CreateRecipe")
            .WithSummary("Create a recipe")
            .WithDescription("Creates a new recipe with optional ingredients.")
            .Produces<RecipeResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .AddEndpointFilter<ValidationFilter<RecipeRequest>>();
        group.MapGet("/", GetAll)
            .WithName("GetAllRecipes")
            .WithSummary("Get all recipes")
            .WithDescription("Returns a list of all recipes with estimated costs.");
        group.MapGet("/{id:guid}", GetById)
            .WithName("GetRecipeById")
            .WithSummary("Get recipe by id")
            .WithDescription("Returns a single recipe by its id.")
            .Produces<RecipeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateRecipe")
            .WithSummary("Update a recipe")
            .WithDescription("Updates an existing recipe by its id.")
            .Produces<RecipeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .AddEndpointFilter<ValidationFilter<RecipeRequest>>();
        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteRecipe")
            .WithSummary("Delete a recipe")
            .WithDescription("Deletes a recipe by its id.")
            .Produces(StatusCodes.Status204NoContent);
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
