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

public class IngredientServiceTests
{
    private readonly AppDbContext _dbContext;
    private readonly IngredientService _ingredientService;
    private readonly Recipe _recipe;

    public IngredientServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        var ingredientRepository = new IngredientRepository(_dbContext);
        var recipeRepository = new RecipeRepository(_dbContext);
        _ingredientService = new IngredientService(recipeRepository, ingredientRepository);

        // seed a recipe to work with
        _recipe = new Recipe { Name = "Pasta", Description = "A delicious pasta", Servings = 2 };
        _dbContext.Recipes.Add(_recipe);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistIngredientToDatabase()
    {
        // Arrange
        var request = new IngredientRequest("Spaghetti", 200, "g", 0.01m);

        // Act
        var result = await _ingredientService.CreateAsync(_recipe.Id, request, CancellationToken.None);

        // Assert — verify returned response
        result.Id.Should().NotBe(Guid.Empty);
        result.Name.Should().Be("Spaghetti");
        result.TotalCost.Should().Be(2.00m);  // 200 * 0.01

        // Assert — verify actually in database
        var saved = await _dbContext.Ingredients.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.RecipeId.Should().Be(_recipe.Id);
    }

    [Fact]
    public async Task AddAsync_ShouldUpdateRecipeEstimatedCost()
    {
        // Arrange
        await _ingredientService.CreateAsync(_recipe.Id, new IngredientRequest("Spaghetti", 200, "g", 0.01m), CancellationToken.None);
        await _ingredientService.CreateAsync(_recipe.Id, new IngredientRequest("Eggs", 3, "pcs", 0.50m), CancellationToken.None);

        // Act — get recipe to check estimated cost
        var recipe = await _dbContext.Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == _recipe.Id);

        // Assert
        var estimatedCost = recipe!.Ingredients.Sum(i => i.Quantity * i.PricePerUnit);
        estimatedCost.Should().Be(3.50m);  // 200*0.01 + 3*0.50
    }

    [Fact]
    public async Task AddAsync_WhenRecipeDoesNotExist_ShouldThrowNotFoundException()
    {
        // Act
        var act = async () => await _ingredientService.CreateAsync(
            Guid.NewGuid(),
            new IngredientRequest("Spaghetti", 200, "g", 0.01m),
            CancellationToken.None
        );

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Recipe*");
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteIngredientFromDatabase()
    {
        // Arrange — add ingredient first
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
        _dbContext.ChangeTracker.Clear();

        // Act
        await _ingredientService.DeleteAsync(_recipe.Id, ingredient.Id, CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        // Assert
        var deleted = await _dbContext.Ingredients.FindAsync(ingredient.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_WhenRecipeDoesNotExist_ShouldThrowNotFoundException()
    {
        var act = async () => await _ingredientService.DeleteAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None
        );
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Recipe*");
    }

    [Fact]
    public async Task RemoveAsync_WhenIngredientDoesNotExist_ShouldThrowNotFoundException()
    {
        var act = async () => await _ingredientService.DeleteAsync(
            _recipe.Id,
            Guid.NewGuid(),
            CancellationToken.None
        );
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Ingredient*");
    }
}

