using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RecipeBudgetService.Infrastructure.Data;
using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Application.Repositories;
using RecipeBudgetService.Infrastructure.Repositories;

namespace RecipeBudgetService.Tests.IntegrationTests.Repositories;

public class IngredientRepositoryTests : IAsyncLifetime
{
    private AppDbContext _dbContext = null!;
    private IngredientRepository _repository = null!;
    private Recipe _recipe = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _repository = new IngredientRepository(_dbContext);
        
        _recipe = new Recipe
        {
            Name = "Pasta",
            Description = "Delicious pasta recipe",
            Servings = 2,
            UserId = Guid.NewGuid()
        };
        _dbContext.Recipes.Add(_recipe);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DisposeAsync() => await _dbContext.DisposeAsync();

    [Fact]
    public async Task CreateAsync_ShouldAddIngredientToRecipe()
    {
        // Arrange
        var ingredient = new Ingredient
        {
            Name = "Spaghetti",
            Quantity = 200,
            Unit = "g",
            PricePerUnit = 0.01m,
            RecipeId = _recipe.Id
        };

        // Act
        var createdIngredient = await _repository.CreateAsync(ingredient, CancellationToken.None);

        // Assert
        createdIngredient.Should().NotBeNull();
        createdIngredient.Id.Should().NotBe(Guid.Empty);
        createdIngredient.Name.Should().Be(ingredient.Name);
        createdIngredient.Quantity.Should().Be(ingredient.Quantity);
        createdIngredient.Unit.Should().Be(ingredient.Unit);
        createdIngredient.PricePerUnit.Should().Be(ingredient.PricePerUnit);

        _dbContext.Ingredients.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_ShouldBeAssociatedWithRecipe()
    {
        // Arrange
        var ingredient = new Ingredient
        {
            Name = "Spaghetti",
            Quantity = 200,
            Unit = "g",
            PricePerUnit = 0.01m,
            RecipeId = _recipe.Id
        };

        // Act
        var createdIngredient = await _repository.CreateAsync(ingredient, CancellationToken.None);

        // Assert
        createdIngredient.RecipeId.Should().Be(_recipe.Id);
    }

    [Fact]
    public async Task DeleteAsync_WhenIngredientExists_ShouldRemoveIngredient()
    {
        // Arrange
        var ingredient = new Ingredient
        {
            Name = "Spaghetti",
            Quantity = 200,
            Unit = "g",
            PricePerUnit = 0.01m,
            RecipeId = _recipe.Id
        };
        _dbContext.Ingredients.Add(ingredient);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(ingredient.Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var deleted = await _dbContext.Ingredients.FindAsync(ingredient.Id, CancellationToken.None);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenIngredientDesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var ingredient = new Ingredient
        {
            Name = "Spaghetti",
            Quantity = 200,
            Unit = "g",
            PricePerUnit = 0.01m,
            RecipeId = _recipe.Id
        };
        _dbContext.Ingredients.Add(ingredient);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _dbContext.Ingredients.Should().HaveCount(1);
    }
}

