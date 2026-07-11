using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RecipeBudgetService.Infrastructure.Data;
using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Domain.Entities;
using System.Net;
using System.Net.Http.Json;

namespace RecipeBudgetService.Tests.IntegrationTests.Endpoints;

public class IngredientEndpointsTests : BaseEndpointsIntegrationTests
{
    private Recipe _recipe = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _recipe = await SeedRecipeAsync();
    }

    private async Task<Recipe> SeedRecipeAsync(
        string name = "Pasta",
        string description = "A delicious pasta",
        int servings = 2,
        List<Ingredient>? ingredients = null)
    {
        return await SeedAsync(async db =>
        {
            var recipe = new Recipe
            {
                Name = name,
                Servings = servings,
                Ingredients = ingredients ?? []
            };
            db.Recipes.Add(recipe);
            await db.SaveChangesAsync();
            return recipe;
        });
    }

    private async Task<Ingredient> SeedIngredientAsync(
        string name = "Spaghetti",
        decimal quantity = 200,
        string unit = "g",
        decimal pricePerUnit = 0.01m)
    {
        return await SeedAsync(async db =>
        {
            var ingredient = new Ingredient
            {
                Name = name,
                Quantity = quantity,
                Unit = unit,
                PricePerUnit = pricePerUnit,
                RecipeId = _recipe.Id
            };
            db.Ingredients.Add(ingredient);
            await db.SaveChangesAsync();
            return ingredient;
        });
    }

    [Fact]
    public async Task POST_Ingredients_WithValidRequest_ShouldReturn201()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/recipes/{_recipe.Id}/ingredients",
            new IngredientRequest("Spaghetti", 200, "g", 0.01m));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task POST_Ingredients_ShouldReturnCreatedIngredientInBody()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/recipes/{_recipe.Id}/ingredients",
            new IngredientRequest("Spaghetti", 200, "g", 0.01m));
        var body = await response.Content
            .ReadFromJsonAsync<IngredientResponse>();

        body!.Id.Should().NotBe(Guid.Empty);
        body.Name.Should().Be("Spaghetti");
        body.Quantity.Should().Be(200);
        body.Unit.Should().Be("g");
        body.PricePerUnit.Should().Be(0.01m);
        body.TotalCost.Should().Be(2.00m);
    }

    [Fact]
    public async Task POST_Ingredients_ShouldReturnLocationHeader()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/recipes/{_recipe.Id}/ingredients",
            new IngredientRequest("Spaghetti", 200, "g", 0.01m));
        var body = await response.Content
            .ReadFromJsonAsync<IngredientResponse>();

        response.Headers.Location!.ToString()
            .Should().Be($"/api/recipes/{_recipe.Id}/ingredients/{body!.Id}");
    }

    [Fact]
    public async Task POST_Ingredients_WhenRecipeDoesNotExist_ShouldReturn404()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/recipes/{Guid.NewGuid()}/ingredients",
            new IngredientRequest("Spaghetti", 200, "g", 0.01m));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_Ingredients_WhenNameIsEmpty_ShouldReturn400()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/recipes/{_recipe.Id}/ingredients",
            new IngredientRequest("", 200, "g", 0.01m));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Ingredients_WhenQuantityIsZero_ShouldReturn400()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/recipes/{_recipe.Id}/ingredients",
            new IngredientRequest("Spaghetti", 0, "g", 0.01m));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Ingredients_WhenUnitIsEmpty_ShouldReturn400()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/recipes/{_recipe.Id}/ingredients",
            new IngredientRequest("Spaghetti", 200, "", 0.01m));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Ingredients_WhenPriceIsNegative_ShouldReturn400()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/recipes/{_recipe.Id}/ingredients",
            new IngredientRequest("Spaghetti", 200, "g", -1m));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DELETE_Ingredients_WhenIngredientExists_ShouldReturn204()
    {
        // Arrange
        var ingredient = await SeedIngredientAsync();

        // Act
        var response = await Client.DeleteAsync(
            $"/api/recipes/{_recipe.Id}/ingredients/{ingredient.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DELETE_Ingredients_WhenIngredientDoesNotExist_ShouldReturn404()
    {
        var response = await Client.DeleteAsync(
            $"/api/recipes/{_recipe.Id}/ingredients/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_Ingredients_WhenRecipeDoesNotExist_ShouldReturn404()
    {
        var ingredient = await SeedIngredientAsync();

        var response = await Client.DeleteAsync(
            $"/api/recipes/{Guid.NewGuid()}/ingredients/{ingredient.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_Ingredients_ShouldRemoveFromDatabase()
    {
        // Arrange
        var ingredient = await SeedIngredientAsync();

        // Act
        await Client.DeleteAsync(
            $"/api/recipes/{_recipe.Id}/ingredients/{ingredient.Id}");

        // Assert — verify gone from db
        await SeedAsync(async db =>
        {
            var deleted = await db.Ingredients.FindAsync(ingredient.Id);
            deleted.Should().BeNull();
            return deleted;
        });
    }

    [Fact]
    public async Task DELETE_Ingredients_ShouldUpdateRecipeEstimatedCost()
    {
        // Arrange — add two ingredients
        var ingredient1 = await SeedIngredientAsync("Spaghetti", 200, "g", 0.01m);
        await SeedIngredientAsync("Eggs", 3, "pcs", 0.50m);

        // check cost before
        var before = await Client.GetAsync($"/api/recipes/{_recipe.Id}");
        var beforeBody = await before.Content.ReadFromJsonAsync<RecipeResponse>();
        beforeBody!.EstimatedCost.Should().Be(3.50m);

        // Act — remove one ingredient
        await Client.DeleteAsync(
            $"/api/recipes/{_recipe.Id}/ingredients/{ingredient1.Id}");

        // Assert — cost updated
        var after = await Client.GetAsync($"/api/recipes/{_recipe.Id}");
        var afterBody = await after.Content.ReadFromJsonAsync<RecipeResponse>();
        afterBody!.EstimatedCost.Should().Be(1.50m);  // only Eggs remain
    }
}

