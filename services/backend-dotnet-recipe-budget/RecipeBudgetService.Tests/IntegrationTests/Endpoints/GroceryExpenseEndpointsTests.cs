using FluentAssertions;
using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Domain.Entities;
using System.Net;
using System.Net.Http.Json;

namespace RecipeBudgetService.Tests.IntegrationTests.Endpoints;

public class GroceryExpenseEndpointsTests : BaseEndpointsIntegrationTests
{
    private Recipe _recipe = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _recipe = await SeedAsync(async db =>
        {
            var recipe = new Recipe { Name = "Pasta", Servings = 2 };
            db.Recipes.Add(recipe);
            await db.SaveChangesAsync();
            return recipe;
        });
    }

    private async Task<GroceryExpense> SeedExpenseAsync(
        string description = "Weekly groceries",
        decimal amount = 50.00m,
        ExpenseCategory category = ExpenseCategory.Groceries,
        Guid? recipeId = null)
    {
        return await SeedAsync(async db =>
        {
            var expense = new GroceryExpense
            {
                Description = description,
                Amount = amount,
                Category = category,
                Date = DateTime.UtcNow,
                RecipeId = recipeId
            };
            db.GroceryExpenses.Add(expense);
            await db.SaveChangesAsync();
            return expense;
        });
    }

    [Fact]
    public async Task GET_ApiExpenses_WhenNoExpenses_ShouldReturn200WithEmptyList()
    {
        var response = await Client.GetAsync("/api/expenses");
        var body = await response.Content
            .ReadFromJsonAsync<List<GroceryExpenseResponse>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task GET_ApiExpenses_WhenExpensesExist_ShouldReturnAll()
    {
        await SeedExpenseAsync("Groceries", 50.00m, ExpenseCategory.Groceries);
        await SeedExpenseAsync("Dinner", 35.00m, ExpenseCategory.EatingOut);

        var response = await Client.GetAsync("/api/expenses");
        var body = await response.Content
            .ReadFromJsonAsync<List<GroceryExpenseResponse>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().HaveCount(2);
    }

    [Fact]
    public async Task GET_ApiExpensesById_WhenExpenseExists_ShouldReturn200()
    {
        var expense = await SeedExpenseAsync();

        var response = await Client.GetAsync($"/api/expenses/{expense.Id}");
        var body = await response.Content
            .ReadFromJsonAsync<GroceryExpenseResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Id.Should().Be(expense.Id);
        body.Description.Should().Be("Weekly groceries");
        body.CategoryName.Should().Be("Groceries");
    }

    [Fact]
    public async Task GET_ApiExpensesById_WhenExpenseDoesNotExist_ShouldReturn404()
    {
        var response = await Client.GetAsync($"/api/expenses/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ApiExpensesSummary_ShouldReturnCorrectTotals()
    {
        await SeedExpenseAsync("Groceries", 50.00m, ExpenseCategory.Groceries);
        await SeedExpenseAsync("Dinner", 35.00m, ExpenseCategory.EatingOut);
        await SeedExpenseAsync("Supplies", 20.00m, ExpenseCategory.Household);

        var response = await Client.GetAsync("/api/expenses/summary");
        var body = await response.Content
            .ReadFromJsonAsync<ExpenseSummaryResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.TotalAmount.Should().Be(105.00m);
        body.TotalCount.Should().Be(3);
        body.Breakdowns.Should().HaveCount(3);
    }

    [Fact]
    public async Task GET_ApiExpensesSummary_WhenNoExpenses_ShouldReturnZeroSummary()
    {
        var response = await Client.GetAsync("/api/expenses/summary");
        var body = await response.Content
            .ReadFromJsonAsync<ExpenseSummaryResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.TotalAmount.Should().Be(0);
        body.TotalCount.Should().Be(0);
        body.Breakdowns.Should().BeEmpty();
    }

    [Fact]
    public async Task GET_ApiExpensesByCategory_ShouldReturnOnlyMatchingCategory()
    {
        await SeedExpenseAsync("Groceries", 50.00m, ExpenseCategory.Groceries);
        await SeedExpenseAsync("Dinner", 35.00m, ExpenseCategory.EatingOut);

        var response = await Client.GetAsync(
            $"/api/expenses/category/{ExpenseCategory.Groceries}");
        var body = await response.Content
            .ReadFromJsonAsync<List<GroceryExpenseResponse>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().HaveCount(1);
        body!.First().Category.Should().Be(ExpenseCategory.Groceries);
    }

    [Fact]
    public async Task POST_ApiExpenses_WithValidRequest_ShouldReturn201()
    {
        var response = await Client.PostAsJsonAsync("/api/expenses",
            new GroceryExpenseRequest("Weekly groceries", 50.00m, ExpenseCategory.Groceries));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task POST_ApiExpenses_ShouldReturnCreatedExpenseInBody()
    {
        var response = await Client.PostAsJsonAsync("/api/expenses",
            new GroceryExpenseRequest("Weekly groceries", 50.00m, ExpenseCategory.Groceries));
        var body = await response.Content
            .ReadFromJsonAsync<GroceryExpenseResponse>();

        body!.Id.Should().NotBe(Guid.Empty);
        body.Description.Should().Be("Weekly groceries");
        body.Amount.Should().Be(50.00m);
        body.Category.Should().Be(ExpenseCategory.Groceries);
        body.CategoryName.Should().Be("Groceries");
        body.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task POST_ApiExpenses_WithValidRecipeId_ShouldReturn201()
    {
        var response = await Client.PostAsJsonAsync("/api/expenses",
            new GroceryExpenseRequest(
                "Pasta ingredients",
                20.00m,
                ExpenseCategory.Groceries,
                RecipeId: _recipe.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task POST_ApiExpenses_WithInvalidRecipeId_ShouldReturn404()
    {
        var response = await Client.PostAsJsonAsync("/api/expenses",
            new GroceryExpenseRequest(
                "Pasta ingredients",
                20.00m,
                ExpenseCategory.Groceries,
                RecipeId: Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_ApiExpenses_WhenDescriptionIsEmpty_ShouldReturn400()
    {
        var response = await Client.PostAsJsonAsync("/api/expenses",
            new GroceryExpenseRequest("", 50.00m, ExpenseCategory.Groceries));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ApiExpenses_WhenAmountIsZero_ShouldReturn400()
    {
        var response = await Client.PostAsJsonAsync("/api/expenses",
            new GroceryExpenseRequest("Groceries", 0, ExpenseCategory.Groceries));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ApiExpenses_WhenAmountIsNegative_ShouldReturn400()
    {
        var response = await Client.PostAsJsonAsync("/api/expenses",
            new GroceryExpenseRequest("Groceries", -1m, ExpenseCategory.Groceries));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_ApiExpenses_WhenDateIsInFuture_ShouldReturn400()
    {
        var response = await Client.PostAsJsonAsync("/api/expenses",
            new GroceryExpenseRequest(
                "Groceries",
                50.00m,
                ExpenseCategory.Groceries,
                Date: DateTime.UtcNow.AddDays(1)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PUT_ApiExpensesById_WithValidRequest_ShouldReturn200()
    {
        var expense = await SeedExpenseAsync();

        var response = await Client.PutAsJsonAsync($"/api/expenses/{expense.Id}",
            new GroceryExpenseRequest("Updated groceries", 75.00m, ExpenseCategory.Household));
        var body = await response.Content
            .ReadFromJsonAsync<GroceryExpenseResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Description.Should().Be("Updated groceries");
        body.Amount.Should().Be(75.00m);
        body.Category.Should().Be(ExpenseCategory.Household);
    }

    [Fact]
    public async Task PUT_ApiExpensesById_WhenExpenseDoesNotExist_ShouldReturn404()
    {
        var response = await Client.PutAsJsonAsync($"/api/expenses/{Guid.NewGuid()}",
            new GroceryExpenseRequest("Updated", 75.00m, ExpenseCategory.Household));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PUT_ApiExpensesById_WhenDescriptionIsEmpty_ShouldReturn400()
    {
        var expense = await SeedExpenseAsync();

        var response = await Client.PutAsJsonAsync($"/api/expenses/{expense.Id}",
            new GroceryExpenseRequest("", 75.00m, ExpenseCategory.Household));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DELETE_ApiExpensesById_WhenExpenseExists_ShouldReturn204()
    {
        var expense = await SeedExpenseAsync();

        var response = await Client.DeleteAsync($"/api/expenses/{expense.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DELETE_ApiExpensesById_WhenExpenseDoesNotExist_ShouldReturn204()
    {
        // always returns 204 for security
        var response = await Client.DeleteAsync($"/api/expenses/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DELETE_ApiExpensesById_ShouldRemoveFromDatabase()
    {
        var expense = await SeedExpenseAsync();

        await Client.DeleteAsync($"/api/expenses/{expense.Id}");

        var getResponse = await Client.GetAsync($"/api/expenses/{expense.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

