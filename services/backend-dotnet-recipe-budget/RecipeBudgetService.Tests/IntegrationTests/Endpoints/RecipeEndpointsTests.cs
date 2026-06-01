using FluentAssertions;
using RecipeBudgetService.DTOs;
using RecipeBudgetService.Entities;
using System.Net;
using System.Net.Http.Json;

namespace RecipeBudgetService.Tests.IntegrationTests.Endpoints;

public class RecipeEndpointsTests : BaseEndpointsIntegrationTests
{
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

    [Fact]
    public async Task POST_ApiRecipes_WithValidRequest_ShouldReturn201()
    {
        // Arrange
        var request = new RecipeRequest("Test Recipe", "A test recipe description", 4);

        // Act
        var response = await Client.PostAsJsonAsync("/api/recipes", request, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task POST_ApiRecipes_ShouldReturnCreatedRecipeInBody()
    {
        // Arrange
        var request = new RecipeRequest("Pasta", "A delicious pasta", 2);

        // Act
        var response = await Client.PostAsJsonAsync("/api/recipes", request, CancellationToken.None);
        var body = await response.Content.ReadFromJsonAsync<RecipeResponse>(CancellationToken.None);

        // Assert
        body.Should().NotBeNull();
        body!.Id.Should().NotBe(Guid.Empty);
        body.Name.Should().Be("Pasta");
        body.Servings.Should().Be(2);
        body.EstimatedCost.Should().Be(0);
        body.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task POST_ApiRecipes_ShouldReturnLocationHeader()
    {
        // Arrange
        var request = new RecipeRequest("Pasta", "A delicious pasta", 2);

        // Act
        var response = await Client.PostAsJsonAsync("/api/recipes", request, CancellationToken.None);
        var body = await response.Content.ReadFromJsonAsync<RecipeResponse>(CancellationToken.None);

        // Assert
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString()
            .Should().Be($"/api/recipes/{body!.Id}");
    }

    [Fact]
    public async Task POST_ApiRecipes_WithIngredients_ShouldReturnEstimatedCosts()
    {
        var request = new RecipeRequest(
            "Test Recipe",
            "A test recipe description",
            4,
            [
                new IngredientRequest("Spaghetti", 200, "g", 0.01m),
                new IngredientRequest("Eggs", 3, "pcs", 0.50m)
            ]
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/recipes", request, CancellationToken.None);

        var body = await response.Content.ReadFromJsonAsync<RecipeResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body!.EstimatedCost.Should().Be(3.50m);  // 200*0.01 + 3*0.50
        body.Ingredients.Should().HaveCount(2);
    }

    [Fact]
    public async Task POST_ApiRecipes_WhenNameAlreadyExists_ShouldReturn409()
    {
        // Arrange — create first recipe
        await Client.PostAsJsonAsync("/api/recipes", new RecipeRequest("Pasta", "A test recipe description", 2));

        // Act — try to create same name
        var response = await Client.PostAsJsonAsync("/api/recipes",
            new RecipeRequest("Pasta", "Another test recipe description", 4));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task POST_ApiRecipes_WhenNameIsEmpty_ShouldReturn400()
    {
        // Arrange
        var request = new RecipeRequest("", "A test recipe description", 2);

        // Act
        var response = await Client.PostAsJsonAsync("/api/recipes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ApiRecipes_WhenServingsIsZero_ShouldReturn400()
    {
        // Arrange
        var request = new RecipeRequest("Pasta", "A test recipe description", 0);

        // Act
        var response = await Client.PostAsJsonAsync("/api/recipes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ApiRecipes_WhenRequestIsEmpty_ShouldReturn400()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/recipes", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
