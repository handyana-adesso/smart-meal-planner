using FluentAssertions;
using Moq;
using RecipeBudgetService.Domain.Exceptions;
using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Application.Repositories;
using RecipeBudgetService.Application.Services;
using RecipeBudgetService.Tests.Fixtures;

namespace RecipeBudgetService.Tests.UnitTests.Services;

public class ExpenseServiceTests : IClassFixture<ExpenseServiceFixture>
{
    private readonly Mock<IExpenseRepository> _repositoryMock = new();
    private readonly Mock<IRecipeRepository> _recipeRepositoryMock = new();
    private readonly ExpenseService _service;
    private readonly List<GroceryExpense> _expenses;
    private readonly Recipe _recipe;
    private readonly Guid _userId;

    public ExpenseServiceTests(ExpenseServiceFixture fixture)
    {
        _expenses = fixture.Expenses;
        _recipe = fixture.Recipe;
        _userId = fixture.UserId;
        _service = new ExpenseService(
            _repositoryMock.Object,
            _recipeRepositoryMock.Object);

        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expenses);

        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Guid userId, CancellationToken ct) => _expenses.FirstOrDefault(e => e.Id == id));

        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<GroceryExpense>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroceryExpense expense, CancellationToken ct) => new()
            {
                Id = Guid.NewGuid(),
                Description = expense.Description,
                Amount = expense.Amount,
                Category = expense.Category,
                Date = expense.Date,
                CreatedAt = DateTime.UtcNow,
                RecipeId = expense.RecipeId,
                UserId = expense.UserId
            });

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<GroceryExpense>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroceryExpense expense, Guid userId, CancellationToken ct) => _expenses.FirstOrDefault(e => e.Id == expense.Id));

        _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Guid userId, CancellationToken ct) => _expenses.Any(e => e.Id == id));

        _repositoryMock.Setup(r => r.GetByCategoryAsync(It.IsAny<ExpenseCategory>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExpenseCategory category, Guid userId, CancellationToken ct) => _expenses.Where(e => e.Category == category).ToList());

        _recipeRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Guid userId, CancellationToken ct) => id == _recipe.Id ? _recipe : null);
    }

    [Fact]
    public void Constructor_WhenRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        var act = () => new ExpenseService(null!, _recipeRepositoryMock.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("expenseRepository");
    }

    [Fact]
    public void Constructor_WhenRecipeRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        var act = () => new ExpenseService(_repositoryMock.Object, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("recipeRepository");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllMappedExpenses()
    {
        var result = await _service.GetAllAsync(_userId, CancellationToken.None);
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedCategoryNames()
    {
        var result = await _service.GetAllAsync(_userId, CancellationToken.None);
        result.Should().AllSatisfy(e =>
            e.CategoryName.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public async Task GetByIdAsync_WhenExpenseExists_ShouldReturnMappedResponse()
    {
        var id = _expenses.First().Id;
        var result = await _service.GetByIdAsync(id, _userId, CancellationToken.None);

        result.Should().NotBeNull();
        result.Description.Should().Be("Weekly groceries");
        result.Amount.Should().Be(50.00m);
        result.Category.Should().Be(ExpenseCategory.Groceries);
        result.CategoryName.Should().Be("Groceries");
    }

    [Fact]
    public async Task GetByIdAsync_WhenExpenseDoesNotExist_ShouldThrowNotFoundException()
    {
        var act = async () => await _service.GetByIdAsync(Guid.NewGuid(), _userId, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        var act = async () => await _service.GetByIdAsync(Guid.Empty, _userId, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnMappedResponse()
    {
        var request = new GroceryExpenseRequest(
            "Supermarket run",
            45.00m,
            ExpenseCategory.Groceries);

        var result = await _service.CreateAsync(request, _userId, CancellationToken.None);

        result.Id.Should().NotBe(Guid.Empty);
        result.Description.Should().Be("Supermarket run");
        result.Amount.Should().Be(45.00m);
        result.Category.Should().Be(ExpenseCategory.Groceries);
        result.CategoryName.Should().Be("Groceries");
    }

    [Fact]
    public async Task CreateAsync_WithValidRecipeId_ShouldNotThrow()
    {
        var request = new GroceryExpenseRequest(
            "Pasta ingredients",
            20.00m,
            ExpenseCategory.Groceries,
            RecipeId: _recipe.Id);

        var act = async () => await _service.CreateAsync(request, _userId, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateAsync_WithInvalidRecipeId_ShouldThrowNotFoundException()
    {
        var request = new GroceryExpenseRequest(
            "Pasta ingredients",
            20.00m,
            ExpenseCategory.Groceries,
            RecipeId: Guid.NewGuid());

        var act = async () => await _service.CreateAsync(request, _userId, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Recipe*");
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
    {
        var act = async () => await _service.CreateAsync(null!, _userId, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task CreateAsync_WhenDescriptionIsEmpty_ShouldThrowArgumentException(
        string? description)
    {
        var act = async () => await _service.CreateAsync(
            new GroceryExpenseRequest(description!, 45.00m, ExpenseCategory.Groceries),
            _userId,
            CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenExpenseExists_ShouldReturnUpdated()
    {
        var id = _expenses.First().Id;
        var result = await _service.UpdateAsync(
            id,
            new GroceryExpenseRequest("Updated groceries", 60.00m, ExpenseCategory.Groceries),
            _userId,
            CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenExpenseDoesNotExist_ShouldThrowNotFoundException()
    {
        var act = async () => await _service.UpdateAsync(
            Guid.NewGuid(),
            new GroceryExpenseRequest("Ghost", 10.00m, ExpenseCategory.Groceries),
            _userId,
            CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        var act = async () => await _service.UpdateAsync(
            Guid.Empty,
            new GroceryExpenseRequest("Test", 10.00m, ExpenseCategory.Groceries),
            _userId,
            CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidRecipeId_ShouldThrowNotFoundException()
    {
        var id = _expenses.First().Id;
        var act = async () => await _service.UpdateAsync(
            id,
            new GroceryExpenseRequest(
                "Test",
                10.00m,
                ExpenseCategory.Groceries,
                RecipeId: Guid.NewGuid()),
            _userId,
            CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Recipe*");
    }

    [Fact]
    public async Task DeleteAsync_WhenExpenseExists_ShouldReturnTrue()
    {
        var id = _expenses.First().Id;
        var result = await _service.DeleteAsync(id, _userId, CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WhenExpenseDoesNotExist_ShouldReturnFalse()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid(), _userId, CancellationToken.None);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
    {
        var act = async () => await _service.DeleteAsync(Guid.Empty, _userId, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnOnlyMatchingCategory()
    {
        var result = await _service.GetByCategoryAsync(ExpenseCategory.Groceries, _userId, CancellationToken.None);

        result.Should().HaveCount(1);
        result.Should().AllSatisfy(e =>
            e.Category.Should().Be(ExpenseCategory.Groceries));
    }

    [Fact]
    public async Task GetByCategoryAsync_WhenNoneInCategory_ShouldReturnEmptyList()
    {
        _repositoryMock
            .Setup(r => r.GetByCategoryAsync(ExpenseCategory.Household, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GroceryExpense>());

        var result = await _service.GetByCategoryAsync(ExpenseCategory.Household, _userId, CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnCorrectTotalAmount()
    {
        var result = await _service.GetSummaryAsync(_userId, CancellationToken.None);

        result.TotalAmount.Should().Be(105.00m);  // 50 + 35 + 20
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnBreakdownPerCategory()
    {
        var result = await _service.GetSummaryAsync(_userId, CancellationToken.None);

        result.Breakdowns.Should().HaveCount(3);
        result.Breakdowns.Should().ContainSingle(b =>
            b.Category == ExpenseCategory.Groceries &&
            b.TotalAmount == 50.00m);
        result.Breakdowns.Should().ContainSingle(b =>
            b.Category == ExpenseCategory.EatingOut &&
            b.TotalAmount == 35.00m);
        result.Breakdowns.Should().ContainSingle(b =>
            b.Category == ExpenseCategory.Household &&
            b.TotalAmount == 20.00m);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldReturnCorrectPercentages()
    {
        var result = await _service.GetSummaryAsync(_userId, CancellationToken.None);

        var groceries = result.Breakdowns
            .First(b => b.Category == ExpenseCategory.Groceries);

        groceries.Percentage.Should().Be(47.62m);  // 50/105 * 100
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldOrderBreakdownByTotalAmountDescending()
    {
        var result = await _service.GetSummaryAsync(_userId, CancellationToken.None);

        result.Breakdowns.Should().BeInDescendingOrder(b => b.TotalAmount);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenNoExpenses_ShouldReturnZeroSummary()
    {
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GroceryExpense>());

        var result = await _service.GetSummaryAsync(_userId, CancellationToken.None);

        result.TotalAmount.Should().Be(0);
        result.TotalCount.Should().Be(0);
        result.Breakdowns.Should().BeEmpty();
    }
}
