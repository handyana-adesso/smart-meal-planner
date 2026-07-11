using FluentAssertions;
using Moq;
using RecipeBudgetService.Domain.Exceptions;
using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Application.Repositories;
using RecipeBudgetService.Application.Services;
using RecipeBudgetService.Tests.Fixtures;

namespace RecipeBudgetService.Tests.UnitTests.Services;

public class IngredientServiceTests : IClassFixture<IngredientServiceFixture>
{
    private readonly Mock<IIngredientRepository> _ingredientRepositoryMock = new();
    private readonly Mock<IRecipeRepository> _recipeRepositoryMock = new();
    private readonly IngredientService _service;
    private readonly Recipe _recipe;

    public IngredientServiceTests(IngredientServiceFixture fixture)
    {
        _recipe = fixture.Recipe;
        _service = new IngredientService(
            _recipeRepositoryMock.Object,
            _ingredientRepositoryMock.Object);

        _recipeRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken ct) => id == _recipe.Id ? _recipe : null);

        _ingredientRepositoryMock
            .Setup(i => i.CreateAsync(It.IsAny<Ingredient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ingredient ingredient, CancellationToken ct) => new Ingredient
            {
                Id = Guid.NewGuid(),
                Name = ingredient.Name,
                Quantity = ingredient.Quantity,
                Unit = ingredient.Unit,
                PricePerUnit = ingredient.PricePerUnit,
                RecipeId = ingredient.RecipeId,
            });

        _ingredientRepositoryMock
            .Setup(i => i.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken ct) => 
                _recipe.Ingredients.Any(i => i.Id == id ));
    }

    [Fact]
    public async Task CreateAsync_WhenRecipeExists_ShouldReturnMappedResponse()
    {
        // Arrange
        var request = new IngredientRequest("Tomato", 2, "pcs", 0.5m);

        // Act
        var result = await _service.CreateAsync(_recipe.Id, request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().NotBe(Guid.Empty);
        result.Name.Should().Be(request.Name);
        result.Quantity.Should().Be(request.Quantity);
        result.Unit.Should().Be(request.Unit);
        result.PricePerUnit.Should().Be(request.PricePerUnit);
        result.TotalCost.Should().Be(request.Quantity * request.PricePerUnit);
    }

    [Fact]
    public async Task CreateAsync_WhenRecipeDoesNotExists_ShouldThrowNotFoundException()
    {
        // Act
        var act = async () => await _service.CreateAsync(Guid.NewGuid(), new IngredientRequest("Tomato", 2, "pcs", 0.5m), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WhenRecipeIdIsEmpty_ShouldThrowArgumentException()
    {
        // Act
        var act = async () => await _service.CreateAsync(Guid.Empty, new IngredientRequest("Tomato", 2, "pcs", 0.5m), CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _service.CreateAsync(_recipe.Id, null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenIngredientExists_ShouldNotThrow()
    {
        // Arrange
        var ingredient = _recipe.Ingredients.First();
        
        // Act
        var act = async () => await _service.DeleteAsync(_recipe.Id, ingredient.Id, CancellationToken.None);
        
        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_WhenRecipeDoesNotExists_ShouldThrowNotFoundException()
    {
        // Act
        var act = async () => await _service.DeleteAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Recipe*");
    }

    [Fact]
    public async Task DeleteAsync_WhenIngredientDoesNotExists_ShouldThrowNotFoundException()
    {
        // Act
        var act = async () => await _service.DeleteAsync(_recipe.Id, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Ingredient*");
    }

    [Fact]
    public async Task DeleteAsync_WhenRecipeIdIsEmpty_ShouldThrowArgumentException()
    {
        // Act
        var act = async () => await _service.DeleteAsync(Guid.Empty, _recipe.Ingredients.First().Id, CancellationToken.None);
        
        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenIngredientIdIsEmpty_ShoulThrowArgumentException()
    {
        // Act
        var act = async () => await _service.DeleteAsync(_recipe.Id, Guid.Empty, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}

