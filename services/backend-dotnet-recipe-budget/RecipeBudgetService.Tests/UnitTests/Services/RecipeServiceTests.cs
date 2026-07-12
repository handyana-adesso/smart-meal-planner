using FluentAssertions;
using Moq;
using RecipeBudgetService.Domain.Exceptions;
using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Application.Repositories;
using RecipeBudgetService.Application.Services;

namespace RecipeBudgetService.Tests.UnitTests.Services;

public class RecipeServiceTests
{
    private readonly Mock<IRecipeRepository> _repositoryMock = new();
    private readonly RecipeService _recipeService;
    private readonly Guid _userId = Guid.NewGuid();

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
            .Setup(repo => repo.GetAllAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Recipe>);

        // Act
        var result = await _recipeService.GetAllAsync(_userId, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenRecipesExist_ShouldReturnMappedResponse()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = Guid.NewGuid(), Name = "Pasta", Description = "A delicious pasta", Servings = 1, Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m } } },
            new() { Id = Guid.NewGuid(), Name = "Pizza", Description = "Pizza margharita", Servings = 2, Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Cheese", Quantity = 100, Unit = "g", PricePerUnit = 0.05m } } }
        };
        _repositoryMock
            .Setup(repo => repo.GetAllAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(recipes);

        // Act
        var result = await _recipeService.GetAllAsync(_userId, CancellationToken.None);

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
            .Setup(repo => repo.GetAllAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Recipe>)null!);

        // Act
        var result = await _recipeService.GetAllAsync(_userId, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WhenRecipeExists_ShouldReturnRecipe()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = Guid.NewGuid(), Name = "Pasta", Description = "A delicious pasta", Servings = 1, Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m } } },
            new() { Id = Guid.NewGuid(), Name = "Pizza", Description = "Pizza margharita", Servings = 2, Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Cheese", Quantity = 100, Unit = "g", PricePerUnit = 0.05m } } }
        };
        _repositoryMock
            .Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Guid userId, CancellationToken cancellationToken) => recipes.FirstOrDefault(r => r.Id == id));

        // Act
        var result = await _recipeService.GetByIdAsync(recipes[0].Id, _userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(recipes[0].Name);
        result.EstimatedCost.Should().Be(recipes[0].Ingredients.Sum(i => i.Quantity * i.PricePerUnit));
    }

    [Fact]
    public async Task GetByIdAsync_WhenRecipeDoesNotExists_ShouldThrowNotFoundException()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = Guid.NewGuid(), Name = "Pasta", Description = "A delicious pasta", Servings = 1, Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m } } },
            new() { Id = Guid.NewGuid(), Name = "Pizza", Description = "Pizza margharita", Servings = 2, Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Cheese", Quantity = 100, Unit = "g", PricePerUnit = 0.05m } } }
        };
        _repositoryMock
            .Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Guid userId, CancellationToken cancellationToken) => recipes.FirstOrDefault(r => r.Id == id));

        // Act
        Func<Task> act = async () => await _recipeService.GetByIdAsync(Guid.NewGuid(), _userId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        // Act
        Func<Task> act = async () => await _recipeService.GetByIdAsync(Guid.Empty, _userId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("id");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnMappedResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new RecipeRequest("Pasta", "A delicious pasta", 1, new List<IngredientRequest> { new("Spaghetti", 200, "g", 0.01m) });
        _repositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<Recipe>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recipe recipe, CancellationToken cancellationToken) => new Recipe()
            {
                Id = id,
                Name = recipe.Name,
                Description = recipe.Description,
                Servings = recipe.Servings,
                Ingredients = recipe.Ingredients,
                UserId = recipe.UserId
            });

        // Act
        var result = await _recipeService.CreateAsync(request, _userId, CancellationToken.None);

        // Assert
        result.Id.Should().Be(id);
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        result.Servings.Should().Be(request.Servings);
        result.Ingredients!.Count().Should().Be(request.Ingredients?.Count ?? 0);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _recipeService.CreateAsync(null!, _userId, CancellationToken.None);

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
        var request = new RecipeRequest(name!, "A delicious pasta", 1, new List<IngredientRequest> { new("Spaghetti", 200, "g", 0.01m) });
        // Act
        Func<Task> act = async () => await _recipeService.CreateAsync(request, _userId, CancellationToken.None);
        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("request.Name");
    }

    [Fact]
    public async Task UpdateAsync_WhenRecipeExists_ShouldReturnUpdated()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = Guid.NewGuid(), Name = "Pasta", Description = "A delicious pasta", Servings = 1, Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m } } },
            new() { Id = Guid.NewGuid(), Name = "Pizza", Description = "Pizza margharita", Servings = 2, Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Cheese", Quantity = 100, Unit = "g", PricePerUnit = 0.05m } } }
        };
        var request = new RecipeRequest("Updated Pasta", "An extra delicious pasta", 1, new List<IngredientRequest> { new("Spaghetti", 200, "g", 0.01m) });
        _repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recipe recipe, Guid userId, CancellationToken cancellationToken) =>
            {
                var found = recipes.FirstOrDefault(r => r.Id == recipe.Id);
                if (found is not null)
                {
                    found.Name = recipe.Name;
                    found.Description = recipe.Description;
                    found.Servings = recipe.Servings;
                    found.Ingredients = recipe.Ingredients;
                }
                return found;
            });
        // Act
        var result = await _recipeService.UpdateAsync(recipes[0].Id, request, _userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(recipes[0].Id);
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        result.Servings.Should().Be(request.Servings);
        result.Ingredients!.Count().Should().Be(request.Ingredients?.Count ?? 0);
    }

    [Fact]
    public async Task UpdateAsync_WhenRecipeDoesNotExist_ShouldThrowNotFoundException()
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
                Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m } }
            }
        };
        _repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Recipe recipe, Guid userId, CancellationToken cancellationToken) => recipes.FirstOrDefault(r => r.Id == recipe.Id));

        // Act
        var act = async () => await _recipeService.UpdateAsync(Guid.NewGuid(), request, _userId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var request = new RecipeRequest("Updated Pasta", "An extra delicious pasta", 1, new List<IngredientRequest> { new("Spaghetti", 200, "g", 0.01m) });

        // Act
        Func<Task> act = async () => await _recipeService.UpdateAsync(Guid.Empty, request, _userId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("id");
    }

    [Fact]
    public async Task UpdateAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _recipeService.UpdateAsync(Guid.NewGuid(), null!, _userId, CancellationToken.None);

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
        var request = new RecipeRequest(name!, "An extra delicious pasta", 1, new List<IngredientRequest> { new("Spaghetti", 200, "g", 0.01m) });

        // Act
        Func<Task> act = async () => await _recipeService.UpdateAsync(Guid.NewGuid(), request, _userId, CancellationToken.None);

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
            new() { Id = Guid.NewGuid(), Name = "Pasta", Description = "A delicious pasta", Servings = 1, Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m } } },
            new() { Id = Guid.NewGuid(), Name = "Pizza", Description = "Pizza margharita", Servings = 2, Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Cheese", Quantity = 100, Unit = "g", PricePerUnit = 0.05m } } }
        };
        _repositoryMock
            .Setup(repo => repo.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Guid userId, CancellationToken cancellationToken) => recipes.Any(r => r.Id == id));

        // Act
        var result = await _recipeService.DeleteAsync(recipes[0].Id, _userId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WhenRecipeDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new() { Id = Guid.NewGuid(), Name = "Pasta", Description = "A delicious pasta", Servings = 1, Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Spaghetti", Quantity = 200, Unit = "g", PricePerUnit = 0.01m } } },
            new() { Id = Guid.NewGuid(), Name = "Pizza", Description = "Pizza margharita", Servings = 2, Ingredients = new List<Ingredient> { new() { Id = Guid.NewGuid(), Name = "Cheese", Quantity = 100, Unit = "g", PricePerUnit = 0.05m } } }
        };
        _repositoryMock
            .Setup(repo => repo.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Guid userId, CancellationToken cancellationToken) => recipes.Any(r => r.Id == id));

        // Act
        var result = await _recipeService.DeleteAsync(Guid.NewGuid(), _userId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        // Act
        Func<Task> act = async () => await _recipeService.DeleteAsync(Guid.Empty, _userId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("id");
    }
}
