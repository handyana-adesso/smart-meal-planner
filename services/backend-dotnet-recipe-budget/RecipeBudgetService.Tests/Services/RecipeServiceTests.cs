using FluentAssertions;
using Moq;
using RecipeBudgetService.DTOs;
using RecipeBudgetService.Entities;
using RecipeBudgetService.Repositories;
using RecipeBudgetService.Services;

namespace RecipeBudgetService.Tests.Services;

public class RecipeServiceTests
{
    private readonly Mock<IRecipeRepository> _repositoryMock = new();
    private readonly RecipeService _recipeService;

    public RecipeServiceTests()
    {
        _recipeService = new(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoRecipes_ShouldReturnEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Recipe>);

        // Act
        var result = await _recipeService.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenRecipesExist_ShouldReturnMappedResponse()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = Guid.NewGuid(), Name = "Pasta", Description = "A delicious pasta", Servings = 1, EstimatedCost = 10.50m },
            new() { Id = Guid.NewGuid(), Name = "Pizza", Description = "Pizza margharita", Servings = 2, EstimatedCost = 15.00m }
        };
        _repositoryMock
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipes);

        // Act
        var result = await _recipeService.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(r => r.Name == "Pasta");
        result.Should().ContainSingle(r => r.Name == "Pizza");
    }

    [Fact]
    public async Task GetByIdAsync_WhenRecipeExists_ShouldReturnRecipe()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = Guid.NewGuid(), Name = "Pasta", Description = "A delicious pasta", Servings = 1, EstimatedCost = 10.50m },
            new() { Id = Guid.NewGuid(), Name = "Pizza", Description = "Pizza margharita", Servings = 2, EstimatedCost = 15.00m }
        };
        _repositoryMock
            .Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken cancellationToken) => recipes.FirstOrDefault(r => r.Id == id));

        // Act
        var result = await _recipeService.GetByIdAsync(recipes[0].Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(recipes[0].Name);
        result.EstimatedCost.Should().Be(recipes[0].EstimatedCost);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRecipeDoesNotExists_ShouldReturnNull()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = Guid.NewGuid(), Name = "Pasta", Description = "A delicious pasta", Servings = 1, EstimatedCost = 10.50m },
            new() { Id = Guid.NewGuid(), Name = "Pizza", Description = "Pizza margharita", Servings = 2, EstimatedCost = 15.00m }
        };
        _repositoryMock
            .Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken cancellationToken) => recipes.FirstOrDefault(r => r.Id == id));

        // Act
        var result = await _recipeService.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
