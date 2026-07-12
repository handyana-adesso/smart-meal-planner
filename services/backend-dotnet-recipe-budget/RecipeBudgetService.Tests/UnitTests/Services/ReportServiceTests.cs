using FluentAssertions;
using Moq;
using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Application.Repositories;
using RecipeBudgetService.Application.Services;

namespace RecipeBudgetService.Tests.UnitTests.Services;

public class ReportServiceTests
{
    private readonly Mock<IExpenseRepository> _repositoryMock = new();
    private readonly ReportService _reportService;
    private readonly Guid _userId = Guid.NewGuid();

    public ReportServiceTests()
    {
        _reportService = new(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetMonthlySpendingReportAsync_WhenNoExpensesInMonth_ShouldReturnZeroReport()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GroceryExpense>());

        // Act
        var result = await _reportService.GetMonthlySpendingReportAsync(5, 2026, _userId, CancellationToken.None);

        // Assert
        result.Month.Should().Be(5);
        result.Year.Should().Be(2026);
        result.TotalAmount.Should().Be(0);
        result.TotalCount.Should().Be(0);
        result.Breakdowns.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMonthlySpendingReportAsync_ShouldOnlyIncludeExpensesInGivenMonthAndYear()
    {
        // Arrange
        var expenses = new List<GroceryExpense>
        {
            new() { Id = Guid.NewGuid(), Description = "May groceries", Amount = 50m, Category = ExpenseCategory.Groceries, Date = new DateTime(2026, 5, 10), UserId = _userId },
            new() { Id = Guid.NewGuid(), Description = "May dinner", Amount = 30m, Category = ExpenseCategory.EatingOut, Date = new DateTime(2026, 5, 20), UserId = _userId },
            new() { Id = Guid.NewGuid(), Description = "April groceries", Amount = 100m, Category = ExpenseCategory.Groceries, Date = new DateTime(2026, 4, 10), UserId = _userId },
            new() { Id = Guid.NewGuid(), Description = "May 2025 groceries", Amount = 200m, Category = ExpenseCategory.Groceries, Date = new DateTime(2025, 5, 10), UserId = _userId }
        };
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses);

        // Act
        var result = await _reportService.GetMonthlySpendingReportAsync(5, 2026, _userId, CancellationToken.None);

        // Assert
        result.TotalAmount.Should().Be(80m);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetMonthlySpendingReportAsync_ShouldGroupByCategory()
    {
        // Arrange
        var expenses = new List<GroceryExpense>
        {
            new() { Id = Guid.NewGuid(), Description = "Groceries 1", Amount = 60m, Category = ExpenseCategory.Groceries, Date = new DateTime(2026, 5, 1), UserId = _userId },
            new() { Id = Guid.NewGuid(), Description = "Groceries 2", Amount = 40m, Category = ExpenseCategory.Groceries, Date = new DateTime(2026, 5, 2), UserId = _userId },
            new() { Id = Guid.NewGuid(), Description = "Dinner", Amount = 20m, Category = ExpenseCategory.EatingOut, Date = new DateTime(2026, 5, 3), UserId = _userId }
        };
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses);

        // Act
        var result = await _reportService.GetMonthlySpendingReportAsync(5, 2026, _userId, CancellationToken.None);

        // Assert
        result.TotalAmount.Should().Be(120m);
        result.Breakdowns.Should().HaveCount(2);
        result.Breakdowns.Should().ContainSingle(b => b.Category == ExpenseCategory.Groceries && b.TotalAmount == 100m && b.Count == 2);
        result.Breakdowns.Should().ContainSingle(b => b.Category == ExpenseCategory.EatingOut && b.TotalAmount == 20m && b.Count == 1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public async Task GetMonthlySpendingReportAsync_WhenMonthIsOutOfRange_ShouldThrowArgumentException(int month)
    {
        // Act
        Func<Task> act = async () => await _reportService.GetMonthlySpendingReportAsync(month, 2026, _userId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("month");
    }

    [Fact]
    public async Task GetMonthlySpendingReportAsync_WhenYearIsTooOld_ShouldThrowArgumentException()
    {
        // Act
        Func<Task> act = async () => await _reportService.GetMonthlySpendingReportAsync(5, 1999, _userId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("year");
    }

    [Fact]
    public async Task GetMonthlySpendingReportAsync_WhenUserIdIsEmpty_ShouldThrowArgumentException()
    {
        // Act
        Func<Task> act = async () => await _reportService.GetMonthlySpendingReportAsync(5, 2026, Guid.Empty, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("userId");
    }
}
