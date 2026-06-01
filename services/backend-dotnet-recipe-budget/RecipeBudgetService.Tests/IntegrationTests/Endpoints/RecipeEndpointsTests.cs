using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RecipeBudgetService.Data;
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

    [Fact]
    public async Task GET_ApiRecipes_WhenNoRecipes_ShouldReturn200WithEmptyList()
    {
        // Act
        var response = await Client.GetAsync("/api/recipes");
        var body = await response.Content.ReadFromJsonAsync<List<RecipeResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task GET_ApiRecipes_WhenRecipeExist_ShouldReturnAll()
    {
        // Arrange
        await SeedRecipeAsync();
        await SeedRecipeAsync("Pizza", "An original pizza recipe", 1);

        // Act
        var response = await Client.GetAsync("/api/recipes");
        var body = await response.Content.ReadFromJsonAsync<List<RecipeResponse>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().HaveCount(2);
    }

    [Fact]
    public async Task GET_ApiRecipes_ShouldIncludeEstimatedCost()
    {
        // Arrange
        await SeedRecipeAsync(ingredients: [
            new Ingredient { Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m },
            new Ingredient { Name = "Eggs", Quantity = 3, Unit = "pcs", PricePerUnit = 0.50m }
        ]);

        // Act
        var response = await Client.GetAsync("/api/recipes");
        var body = await response.Content.ReadFromJsonAsync<List<RecipeResponse>>();

        // Assert
        body!.First().EstimatedCost.Should().Be(3.50m);
    }

    [Fact]
    public async Task GET_ApiRecipesById_WhenRecipeExists_ShouldReturn200()
    {
        // Arrange
        var recipe = await SeedRecipeAsync();

        // Act
        var response = await Client.GetAsync($"/api/recipes/{recipe.Id}");
        var body = await response.Content.ReadFromJsonAsync<RecipeResponse>();

        // Arrange
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Id.Should().Be(recipe.Id);
        body.Name.Should().Be("Pasta");
    }

    [Fact]
    public async Task GET_ApiRecipesById_WhenRecipeDoesNotExists_ShouldReturn404()
    {
        // Act
        var response = await Client.GetAsync($"/api/recipes/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ApiRecipesById_WhenIdIsInvalid_ShouldReturn404()
    {
        // Act
        var response = await Client.GetAsync("/api/recipes/not-a-guid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ApiRecipesById_ShouldIncludeIngredients()
    {
        // Arrange
        var recipe = await SeedRecipeAsync(ingredients: [
            new Ingredient { Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m },
            new Ingredient { Name = "Eggs", Quantity = 3, Unit = "pcs", PricePerUnit = 0.50m }
        ]);

        // Act
        var response = await Client.GetAsync($"/api/recipes/{recipe.Id}");
        var body = await response.Content.ReadFromJsonAsync<RecipeResponse>();

        // Assert
        body!.Ingredients.Should().HaveCount(2);
    }

    [Fact]
    public async Task PUT_ApiRecipesById_WithValidRequest_ShouldReturn200()
    {
        // Arrange
        var recipe = await SeedRecipeAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/recipes/{recipe.Id}",
            new RecipeRequest("Updated Pasta", "A more delicious pasta recipe", 4));
        var body = await response.Content.ReadFromJsonAsync<RecipeResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Name.Should().Be("Updated Pasta");
        body.Servings.Should().Be(4);
    }

    [Fact]
    public async Task PUT_ApiRecipesById_WhenRecipeDoesNotExits_ShouldReturn404()
    {
        // Act
        var response = await Client.PutAsJsonAsync($"/api/recipes/{Guid.NewGuid()}",
            new RecipeRequest("Updated Pasta", "A more delicious pasta recipe", 4));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PUT_ApiRecipesById_WhenNameAlreadyExists_ShouldReturn409()
    {
        // Arrange — seed two recipes
        var recipe = await SeedRecipeAsync();
        await SeedRecipeAsync("Pizza", "An original pizza recipe", 1);

        // Act — try to rename Pasta to Pizza
        var response = await Client.PutAsJsonAsync($"/api/recipes/{recipe.Id}",
            new RecipeRequest("Pizza", "A pasta pizza recipe", 2));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PUT_ApiRecipesById_WhenNameIsEmpty_ShouldReturn400()
    {
        var recipe = await SeedRecipeAsync();

        var response = await Client.PutAsJsonAsync($"/api/recipes/{recipe.Id}",
            new RecipeRequest("", "An empty recipe", 2));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PUT_ApiRecipesById_WhenServingsIsZero_ShouldReturn400()
    {
        var recipe = await SeedRecipeAsync();

        var response = await Client.PutAsJsonAsync($"/api/recipes/{recipe.Id}",
            new RecipeRequest("Pasta", "A delicious pasta recipe", 0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DELETE_ApiRecipesById_WhenRecipeExists_ShouldReturn204()
    {
        // Arrange
        var recipe = await SeedRecipeAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/recipes/{recipe.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DELETE_ApiRecipesById_WhenRecipeDoesNotExist_ShouldReturn204()
    {
        // Act
        var response = await Client.DeleteAsync($"/api/recipes/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DELETE_ApiRecipesById_ShouldRemoveFromDatabase()
    {
        // Arrange
        var recipe = await SeedRecipeAsync();

        // Act
        await Client.DeleteAsync($"/api/recipes/{recipe.Id}");

        // Assert — verify gone from db
        var getResponse = await Client.GetAsync($"/api/recipes/{recipe.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_ApiRecipesById_ShouldCascadeDeleteIngredients()
    {
        // Arrange
        var recipe = await SeedRecipeAsync("Pasta", "A delicious pasta", 2, [
            new() { Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m }
        ]);

        // Act
        await Client.DeleteAsync($"/api/recipes/{recipe.Id}");

        // Assert — verify ingredients gone too
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Ingredients.Should().BeEmpty();
    }
}
