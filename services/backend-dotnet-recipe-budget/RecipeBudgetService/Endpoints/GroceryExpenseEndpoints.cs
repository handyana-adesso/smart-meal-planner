using Microsoft.AspNetCore.Mvc;
using RecipeBudgetService.Filters;
using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Application.Services;

namespace RecipeBudgetService.Endpoints;

public static class GroceryExpenseEndpoints
{
    public static void MapGroceryExpenseEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/expenses")
            .WithTags("Expenses");

        group.MapGet("/", GetAllAsync)
            .WithName("GetAllExpenses")
            .WithSummary("Get all expenses")
            .WithDescription("Retrieves a list of all expenses.")
            .Produces<List<GroceryExpenseResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetExpenseById")
            .WithSummary("Get a expense by ID")
            .WithDescription("Retrieves a expense by its unique identifier.")
            .Produces<GroceryExpenseResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/summary", GetSummaryAsync)
            .WithName("GetExpenseSummary")
            .WithSummary("Get expense summary")
            .WithDescription("Retturns total amount and breakdown per category.")
            .Produces<ExpenseSummaryResponse>(StatusCodes.Status200OK);

        group.MapGet("/category/{category}", GetByCategoryAsync)
            .WithName("GetExpensesByCategory")
            .WithSummary("Get expenses by category")
            .WithDescription("Retrieves a list of expenses filtered by category.")
            .Produces<List<GroceryExpenseResponse>>(StatusCodes.Status200OK);

        group.MapPost("/", CreateAsync)
            .WithName("CreateExpense")
            .WithSummary("Create a new expense")
            .WithDescription("Creates a new expense with the provided details.")
            .Produces<GroceryExpenseResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .AddEndpointFilter<ValidationFilter<GroceryExpenseRequest>>();

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateExpense")
            .WithSummary("Update an existing expense")
            .WithDescription("Updates an existing expense with the provided details.")
            .Produces<GroceryExpenseResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .AddEndpointFilter<ValidationFilter<GroceryExpenseRequest>>();

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteExpense")
            .WithSummary("Delete an expense")
            .WithDescription("Deletes an expense by its unique identifier.")
            .Produces(StatusCodes.Status204NoContent);
    }

    static async Task<IResult> GetAllAsync(
        [FromServices] IExpenseService expenseService,
        CancellationToken cancellationToken)
        => Results.Ok(await expenseService.GetAllAsync(cancellationToken));

    static async Task<IResult> GetByIdAsync(
        Guid id,
        [FromServices] IExpenseService expenseService,
        CancellationToken cancellationToken)
        => Results.Ok(await expenseService.GetByIdAsync(id, cancellationToken));

    static async Task<IResult> GetSummaryAsync(
        [FromServices] IExpenseService expenseService,
        CancellationToken cancellationToken)
        => Results.Ok(await expenseService.GetSummaryAsync(cancellationToken));

    static async Task<IResult> GetByCategoryAsync(
        ExpenseCategory category,
        [FromServices] IExpenseService expenseService,
        CancellationToken cancellationToken)
        => Results.Ok(await expenseService.GetByCategoryAsync(category, cancellationToken));

    static async Task<IResult> CreateAsync(
        GroceryExpenseRequest request,
        [FromServices] IExpenseService expenseService,
        CancellationToken cancellationToken)
    {
        var created = await expenseService.CreateAsync(request, cancellationToken);
        return Results.Created($"/api/expenses/{created.Id}", created);
    }

    static async Task<IResult> UpdateAsync(
        Guid id,
        GroceryExpenseRequest request,
        [FromServices] IExpenseService expenseService,
        CancellationToken cancellationToken)
        => Results.Ok(await expenseService.UpdateAsync(id, request, cancellationToken));

    static async Task<IResult> DeleteAsync(
        Guid id,
        [FromServices] IExpenseService expenseService,
        CancellationToken cancellationToken)
    {
        await expenseService.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }
}
