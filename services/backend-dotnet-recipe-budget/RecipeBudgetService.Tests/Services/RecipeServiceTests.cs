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
    public void Constructor_WhenRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new RecipeService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("repository");
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
    public async Task GetAllAsync_WhenRepositoryReturnsNull_ShouldReturnEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Recipe>)null!);

        // Act
        var result = await _recipeService.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
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

    [Fact]
    public async Task GetByIdAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        // Act
        Func<Task> act = async () => await _recipeService.GetByIdAsync(Guid.Empty, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("id");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnMappedResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new RecipeRequest("Pasta", "A delicious pasta", 1, 10.50m);
        _repositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<Recipe>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recipe recipe, CancellationToken cancellationToken) => new Recipe()
            {
                Id = id,
                Name = recipe.Name,
                Description = recipe.Description,
                Servings = recipe.Servings,
                EstimatedCost = recipe.EstimatedCost
            });

        // Act
        var result = await _recipeService.CreateAsync(request, CancellationToken.None);

        // Assert
        result.Id.Should().Be(id);
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        result.Servings.Should().Be(request.Servings);
        result.EstimatedCost.Should().Be(request.EstimatedCost);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _recipeService.CreateAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task CreateAsync_WhenNameIsNullOrWhiteSpace_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var request = new RecipeRequest(name!, "A delicious pasta", 1, 10.50m);
        // Act
        Func<Task> act = async () => await _recipeService.CreateAsync(request, CancellationToken.None);
        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("request.Name");
    }

    [Fact]
    public async Task CreateAsync_WhenEstimatedCostIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new RecipeRequest("Pasta", "A delicious pasta", 1, -10.50m);

        // Act
        Func<Task> act = async () => await _recipeService.CreateAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("request.EstimatedCost");
    }

    [Fact]
    public async Task UpdateAsync_WhenRecipeExists_ShouldReturnUpdated()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = Guid.NewGuid(), Name = "Pasta", Description = "A delicious pasta", Servings = 1, EstimatedCost = 10.50m },
            new() { Id = Guid.NewGuid(), Name = "Pizza", Description = "Pizza margharita", Servings = 2, EstimatedCost = 15.00m }
        };
        var request = new RecipeRequest("Updated Pasta", "An extra delicious pasta", 1, 11.50m);
        _repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recipe recipe, CancellationToken cancellationToken) =>
            {
                var found = recipes.FirstOrDefault(r => r.Id == recipe.Id);
                if (found is not null)
                {
                    found.Name = recipe.Name;
                    found.Description = recipe.Description;
                    found.Servings = recipe.Servings;
                    found.EstimatedCost = recipe.EstimatedCost;
                }
                return found;
            });
        // Act
        var result = await _recipeService.UpdateAsync(recipes[0].Id, request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(recipes[0].Id);
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        result.Servings.Should().Be(request.Servings);
        result.EstimatedCost.Should().Be(request.EstimatedCost);
    }

    [Fact]
    public async Task UpdateAsync_WhenRecipeDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var request = new RecipeRequest("Ghost");
        var recipes = new List<Recipe>
        { 
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Pasta",
                Description = "A delicious pasta",
                Servings = 1,
                EstimatedCost = 10.50m
            }
        };
        _repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recipe recipe, CancellationToken cancellationToken) => recipes.FirstOrDefault(r => r.Id == recipe.Id));

        // Act
        var result = await _recipeService.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new RecipeRequest("Updated Pasta", "An extra delicious pasta", 1, 11.50m);

        // Act
        Func<Task> act = async () => await _recipeService.UpdateAsync(Guid.Empty, request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("id");
    }

    [Fact]
    public async Task UpdateAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _recipeService.UpdateAsync(Guid.NewGuid(), null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task UpdateAsync_WhenNameIsNullOrWhiteSpace_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var request = new RecipeRequest(name!, "An extra delicious pasta", 1, 11.50m);

        // Act
        Func<Task> act = async () => await _recipeService.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("request.Name");
    }

    [Fact]
    public async Task DeleteAsync_WhenRecipeDoesExist_ShouldReturnTrue()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = Guid.NewGuid(), Name = "Pasta", Description = "A delicious pasta", Servings = 1, EstimatedCost = 10.50m },
            new() { Id = Guid.NewGuid(), Name = "Pizza", Description = "Pizza margharita", Servings = 2, EstimatedCost = 15.00m }
        };
        _repositoryMock
            .Setup(repo => repo.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken cancellationToken) => recipes.Any(r => r.Id == id));

        // Act
        var result = await _recipeService.DeleteAsync(recipes[0].Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WhenRecipeDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = Guid.NewGuid(), Name = "Pasta", Description = "A delicious pasta", Servings = 1, EstimatedCost = 10.50m },
            new() { Id = Guid.NewGuid(), Name = "Pizza", Description = "Pizza margharita", Servings = 2, EstimatedCost = 15.00m }
        };
        _repositoryMock
            .Setup(repo => repo.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken cancellationToken) => recipes.Any(r => r.Id == id));

        // Act
        var result = await _recipeService.DeleteAsync(Guid.NewGuid(), CancellationToken.None);
        
        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        // Act
        Func<Task> act = async () => await _recipeService.DeleteAsync(Guid.Empty, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("id");
    }
}
