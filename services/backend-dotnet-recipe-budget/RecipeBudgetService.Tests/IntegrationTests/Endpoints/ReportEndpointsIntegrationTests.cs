using FluentAssertions;
using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Domain.Entities;
using System.Net;
using System.Net.Http.Json;

namespace RecipeBudgetService.Tests.IntegrationTests.Endpoints;

public class ReportEndpointsIntegrationTests : BaseEndpointsIntegrationTests
{
    private async Task SeedExpenseAsync(DateTime date, decimal amount, ExpenseCategory category, Guid? userId = null)
    {
        await SeedAsync(async db =>
        {
            db.GroceryExpenses.Add(new GroceryExpense
            {
                Description = "Test expense",
                Amount = amount,
                Category = category,
                Date = date,
                UserId = userId ?? UserId
            });
            await db.SaveChangesAsync();
            return true;
        });
    }

    [Fact]
    public async Task GET_ApiReportsMonthlySpending_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var anonymousClient = Factory.CreateClient();

        // Act
        var response = await anonymousClient.GetAsync("/api/reports/monthly-spending?month=5&year=2026");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_ApiReportsMonthlySpending_WhenNoExpenses_ShouldReturnZeroReport()
    {
        // Act
        var response = await Client.GetAsync("/api/reports/monthly-spending?month=5&year=2026");
        var body = await response.Content.ReadFromJsonAsync<MonthlySpendingReportResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.TotalAmount.Should().Be(0);
        body.TotalCount.Should().Be(0);
        body.Breakdowns.Should().BeEmpty();
    }

    [Fact]
    public async Task GET_ApiReportsMonthlySpending_ShouldOnlyIncludeExpensesInGivenMonth()
    {
        // Arrange
        await SeedExpenseAsync(new DateTime(2026, 5, 10), 50m, ExpenseCategory.Groceries);
        await SeedExpenseAsync(new DateTime(2026, 5, 20), 30m, ExpenseCategory.EatingOut);
        await SeedExpenseAsync(new DateTime(2026, 4, 10), 100m, ExpenseCategory.Groceries);

        // Act
        var response = await Client.GetAsync("/api/reports/monthly-spending?month=5&year=2026");
        var body = await response.Content.ReadFromJsonAsync<MonthlySpendingReportResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.TotalAmount.Should().Be(80m);
        body.TotalCount.Should().Be(2);
        body.Breakdowns.Should().HaveCount(2);
    }

    [Fact]
    public async Task GET_ApiReportsMonthlySpending_ShouldOnlyIncludeCurrentUserExpenses()
    {
        // Arrange
        var (_, otherUserId) = await RegisterAndLoginAsync();
        await SeedExpenseAsync(new DateTime(2026, 5, 10), 50m, ExpenseCategory.Groceries);
        await SeedExpenseAsync(new DateTime(2026, 5, 15), 999m, ExpenseCategory.Groceries, userId: otherUserId);

        // Act
        var response = await Client.GetAsync("/api/reports/monthly-spending?month=5&year=2026");
        var body = await response.Content.ReadFromJsonAsync<MonthlySpendingReportResponse>();

        // Assert
        body!.TotalAmount.Should().Be(50m);
        body.TotalCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public async Task GET_ApiReportsMonthlySpending_WhenMonthIsInvalid_ShouldReturn400(int month)
    {
        // Act
        var response = await Client.GetAsync($"/api/reports/monthly-spending?month={month}&year=2026");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_ApiReportsMonthlySpending_WhenYearIsInvalid_ShouldReturn400()
    {
        // Act
        var response = await Client.GetAsync("/api/reports/monthly-spending?month=5&year=1999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
