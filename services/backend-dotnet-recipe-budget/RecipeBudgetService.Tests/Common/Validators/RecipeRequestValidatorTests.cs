using FluentAssertions;
using RecipeBudgetService.Common.Validators;
using RecipeBudgetService.DTOs;

namespace RecipeBudgetService.Tests.Common.Validators;

public class RecipeRequestValidatorTests
{
    private readonly RecipeRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new RecipeRequest(
            "Pasta",
            "A delicious pasta",
            1,
            10.50m
        );
        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenNameIsInvalid_ShouldHaveErrors(string? name)
    {
        // Arrange
        var request = new RecipeRequest(
            name!,
            "A delicious pasta",
            1,
            10.50m
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RecipeRequest.Name));
    }

    [Fact]
    public void Validate_WhenServingsIsNegative_ShouldHaveErrors()
    {
        // Arrange
        var request = new RecipeRequest(
            "Pasta",
            "A delicious pasta",
            -1,
            10.50m
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RecipeRequest.Servings));
    }

    [Fact]
    public void Validate_WhenEstimatedCostIsNegative_ShouldHaveErrors()
    {
        // Arrange
        var request = new RecipeRequest(
            "Pasta",
            "A delicious pasta",
            1,
            -5.00m
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RecipeRequest.EstimatedCost));
    }

    [Fact]
    public void Validate_WhenNameExceedsMaxLength_ShouldHaveErrors()
    {
        // Arrange
        var longName = new string('A', 101);
        var request = new RecipeRequest(
            longName,
            "A delicious pasta",
            1,
            10.50m
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RecipeRequest.Name));
    }
}
