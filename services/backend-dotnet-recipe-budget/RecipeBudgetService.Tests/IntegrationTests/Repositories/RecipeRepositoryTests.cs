using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RecipeBudgetService.Infrastructure.Data;
using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Application.Repositories;
using RecipeBudgetService.Infrastructure.Repositories;

namespace RecipeBudgetService.Tests.IntegrationTests.Repositories;

public class RecipeRepositoryTests : IAsyncLifetime
{
    private readonly DbContextOptions<AppDbContext> _options;
    private AppDbContext _dbContext;
    private RecipeRepository _repository;

    public RecipeRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(_options);
        _repository = new RecipeRepository(_dbContext);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dbContext.DisposeAsync();

    [Fact]
    public async Task GetAllAsync_WhenNoRecipes_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenRecipeExists_ShouldReturnAll()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() 
            { 
                Id = Guid.NewGuid(), 
                Name = "Pasta", 
                Description = "A delicious pasta", 
                Servings = 1, 
                Ingredients = new List<Ingredient>
                {
                    new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m }
                }
            },
            new() 
            { 
                Id = Guid.NewGuid(), 
                Name = "Pizza", 
                Description = "Pizza margharita", 
                Servings = 2
            }
        };

        _dbContext.Recipes.AddRange(recipes);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRecipeExists_ShouldReturnRecipe()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Pasta",
            Description = "A delicious pasta",
            Servings = 1,
            Ingredients = new List<Ingredient>
            {
                new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m }
            }
        };
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(recipe.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(recipe.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRecipeDoesNotExists_ShouldReturnNull()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Pasta",
            Description = "A delicious pasta",
            Servings = 1,
            Ingredients = new List<Ingredient>
            {
                new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m }
            }
        };
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldSaveAndReturnRecipe()
    {
        // Arrange
        var recipe = new Recipe
        {
            Name = "Pasta",
            Description = "A delicious pasta",
            Servings = 1,
            Ingredients = new List<Ingredient>
            {
                new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m }
            }
        };

        // Act
        var result = await _repository.CreateAsync(recipe, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty).And.NotBe(default(Guid));
        result.Name.Should().Be(recipe.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenRecipeExists_ShouldUpdateAndReturnRecipe()
    {
        // Arrange
        var recipe = new Recipe
        {
            Name = "Pasta",
            Description = "A delicious pasta",
            Servings = 1,
            Ingredients = new List<Ingredient>
            {
                new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m }
            }
        };
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        var updatedRecipe = new Recipe
        {
            Id = recipe.Id,
            Name = "Updated Pasta",
            Description = "An updated delicious pasta",
            Servings = 2,
            Ingredients = recipe.Ingredients
        };

        // Act
        var result = await _repository.UpdateAsync(updatedRecipe, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(recipe.Id);
        result.Name.Should().Be(updatedRecipe.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenRecipeDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var updatedRecipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Updated Pasta",
            Description = "An updated delicious pasta",
            Servings = 2,
            Ingredients = new List<Ingredient>
            {
                new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m }
            }
        };

        // Act
        var result = await _repository.UpdateAsync(updatedRecipe, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenRecipeExists_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Pasta",
            Description = "A delicious pasta",
            Servings = 1,
            Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m } }
        };
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(recipe.Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var deleted = await _repository.GetByIdAsync(recipe.Id, CancellationToken.None);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenRecipeDoesNotExists_ShouldReturnFalse()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Pasta",
            Description = "A delicious pasta",
            Servings = 1,
            Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m } }
        };
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _dbContext.Recipes.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExistsByNameAsync_WhenRecipeExists_ShouldReturnTrue()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Pasta",
            Description = "A delicious pasta",
            Servings = 1,
            Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m } }
        };
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByNameAsync(recipe.Name, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_WhenRecipeDoesNotExists_ShouldReturnFalse()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Name = "Pasta",
            Description = "A delicious pasta",
            Servings = 1,
            Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m } }
        };
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByNameAsync("NonExistingRecipe", CancellationToken.None);
        
        // Assert
        result.Should().BeFalse();
    }
}

