using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RecipeBudgetService.Domain.Exceptions;
using RecipeBudgetService.Infrastructure.Data;
using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Application.Repositories;
using RecipeBudgetService.Infrastructure.Repositories;
using RecipeBudgetService.Application.Services;

namespace RecipeBudgetService.Tests.IntegrationTests.Services;

public class RecipeServiceTests
{
    private readonly AppDbContext _dbContext;
    private readonly RecipeService _recipeService;

    public RecipeServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);
        var repository = new RecipeRepository(_dbContext);
        _recipeService = new RecipeService(repository);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoRecipes_ReturnsEmptyList()
    {
        // Act
        var result = await _recipeService.GetAllAsync(CancellationToken.None);
        
        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenRecipeExists_ShouldReturnAllWithEstimatedCosts()
    {
        // Arrange
        _dbContext.Recipes.Add(new Recipe
        {
            Name = "Pasta",
            Description = "Delicious pasta recipe",
            Ingredients = [
                new Ingredient { Name = "Pasta", Quantity = 200, Unit = "g", PricePerUnit = 0.01m },
            ]
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _recipeService.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().EstimatedCost.Should().Be(2.00m);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRecipeExists_ShouldReturnWithEstimatedCosts()
    {
        // Arrange
        var recipe = new Recipe
        {
            Name = "Pasta",
            Description = "Delicious pasta recipe",
            Servings = 2,
            Ingredients = [
                new Ingredient { Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m },
                new Ingredient { Name = "Eggs", Quantity = 3, Unit = "pcs", PricePerUnit = 0.50m }
            ]
        };
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _recipeService.GetByIdAsync(recipe.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Pasta");
        result.Servings.Should().Be(2);
        result.EstimatedCost.Should().Be(2.00m + 1.50m); // 200g * $0.01 + 3pcs * $0.50)
        result.Ingredients.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRecipeDoesNotExist_ShouldThrowNotFoundException()
    {
        // Act
        Func<Task> act = async () => await _recipeService.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        // Act
        Func<Task> act = async () => await _recipeService.GetByIdAsync(Guid.Empty, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistToDatabase()
    {
        // Arrange
        var request = new RecipeRequest("Pasta", "Delicious pasta recipe", 2);

        // Act
        var result = await _recipeService.CreateAsync(request, CancellationToken.None);

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
        result.Name.Should().Be("Pasta");
        result.Description.Should().Be("Delicious pasta recipe");
        result.Servings.Should().Be(2);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var saved = await _dbContext.Recipes.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Pasta");
    }

    [Fact]
    public async Task CreateAsync_WithIngredients_ShouldPersistIngredientsToo()
    {
        // Arrange
        var request = new RecipeRequest("Pasta", "Delicious pasta recipe", 2, new List<IngredientRequest>
        {
            new("Spaghetti", 200, "g", 0.01m),
            new("Eggs", 3, "pcs", 0.50m)
        });

        // Act
        var result = await _recipeService.CreateAsync(request, CancellationToken.None);

        // Assert
        result.Ingredients.Should().HaveCount(2);
        result.EstimatedCost.Should().Be(2.00m + 1.50m); // 200g * $0.01 + 3pcs * $0.50)

        _dbContext.Ingredients.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var existing = new Recipe { Name = "Pasta", Description = "Existing recipe", Servings = 2 };
        _dbContext.Recipes.Add(existing);
        await _dbContext.SaveChangesAsync();
        var request = new RecipeRequest("Pasta", "New recipe with same name", 4);

        // Act
        Func<Task> act = async () => await _recipeService.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*Pasta*");
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChangesToDatabase()
    {
        // Arrange
        var recipe = new Recipe { Name = "Pasta", Description = "Delicious pasta recipe", Servings = 2 };
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _recipeService.UpdateAsync(
            recipe.Id,
            new RecipeRequest("Updated Pasta", "Updated pasta recipe", 4),
            CancellationToken.None
        );

        // Assert — verify returned response
        result!.Name.Should().Be("Updated Pasta");
        result.Description.Should().Be("Updated pasta recipe");
        result.Servings.Should().Be(4);

        // Assert — verify actually updated in database
        var saved = await _dbContext.Recipes.FindAsync(recipe.Id);
        saved!.Name.Should().Be("Updated Pasta");
        saved.Servings.Should().Be(4);
    }

    [Fact]
    public async Task UpdateAsync_WhenRecipeDoesNotExist_ShouldThrowNotFoundException()
    {
        // Act
        var act = async () => await _recipeService.UpdateAsync(
            Guid.NewGuid(),
            new RecipeRequest("Ghost", "Ghost recipe", 1),
            CancellationToken.None
        );

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveFromDatabase()
    {
        // Arrange
        var recipe = new Recipe { Name = "Pasta", Description = "Delicious pasta recipe", Servings = 2 };
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        // Act
        await _recipeService.DeleteAsync(recipe.Id, CancellationToken.None);

        // Assert — verify actually deleted from database
        var deleted = await _dbContext.Recipes.FindAsync(recipe.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldCascadeDeleteIngredients()
    {
        // Arrange
        var recipe = new Recipe
        {
            Name = "Pasta",
            Description = "Delicious pasta recipe",
            Servings = 2,
            Ingredients = new List<Ingredient>
            {
                new() { Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m }
            }
        };
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        // Act
        await _recipeService.DeleteAsync(recipe.Id, CancellationToken.None);

        // Assert — ingredients deleted too
        _dbContext.Ingredients.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_WhenRecipeDoesNotExist_ShouldReturnsFalse()
    {
        // Act
        var result = await _recipeService.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}

