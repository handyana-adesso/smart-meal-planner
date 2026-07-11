using RecipeBudgetService.Domain.Exceptions;
using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Application.Extensions;
using RecipeBudgetService.Application.Mappers;
using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Application.Repositories;

namespace RecipeBudgetService.Application.Services;

public class ExpenseService(
    IExpenseRepository expenseRepository,
    IRecipeRepository recipeRepository) : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository =
        expenseRepository ?? throw new ArgumentNullException(nameof(expenseRepository));
    private readonly IRecipeRepository _recipeRepository =
        recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));

    public async Task<GroceryExpenseResponse> CreateAsync(GroceryExpenseRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Description, nameof(request.Description));

        // Validate recipe exists if provided
        await ValidateRecipeExists(request.RecipeId, cancellationToken);

        var expense = request.ToEntity();
        var created = await _expenseRepository.CreateAsync(expense, cancellationToken);
        return created.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        GuardExtensions.ThrowIfGuidEmpty(id);
        return await _expenseRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<List<GroceryExpenseResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var expenses = await _expenseRepository.GetAllAsync(cancellationToken);
        return expenses.ToResponses()
            .ToList();
    }

    public async Task<List<GroceryExpenseResponse>> GetByCategoryAsync(ExpenseCategory category, CancellationToken cancellationToken)
    {
        var expenses = await _expenseRepository.GetByCategoryAsync(category, cancellationToken);
        return expenses.ToResponses()
            .ToList();
    }

    public async Task<GroceryExpenseResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        GuardExtensions.ThrowIfGuidEmpty(id);

        var expense = await _expenseRepository.GetByIdAsync(id, cancellationToken);
        if (expense is null)
        {
            throw new NotFoundException($"Expense with ID {id} was not found.");
        }

        return expense.ToResponse();
    }

    public async Task<GroceryExpenseResponse> UpdateAsync(Guid id, GroceryExpenseRequest request, CancellationToken cancellationToken)
    {
        GuardExtensions.ThrowIfGuidEmpty(id);
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Description, nameof(request.Description));

        // Validate recipe exists if provided
        await ValidateRecipeExists(request.RecipeId, cancellationToken);

        var expense = request.ToEntity(id);
        var updated = await _expenseRepository.UpdateAsync(expense, cancellationToken);
        if (updated is null)
        {
            throw new NotFoundException($"Expense with ID {id} was not found.");
        }

        return updated.ToResponse();
    }

    public async Task<ExpenseSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var expenses = await _expenseRepository.GetAllAsync(cancellationToken);

        if (!expenses.Any())
        {
            return new ExpenseSummaryResponse(
                0,
                0,
                []);
        }

        var totalAmount = expenses.Sum(e => e.Amount);
        var totalCount = expenses.Count;

        var breakdown = expenses
            .GroupBy(e => e.Category)
            .Select(g => new ExpenseCategoryBreakdown(
                g.Key,
                g.Key.ToString(),
                g.Sum(e => e.Amount),
                g.Count(),
                totalAmount > 0
                    ? Math.Round(g.Sum(e => e.Amount) / totalAmount * 100, 2)
                    : 0
            ))
            .OrderByDescending(b => b.TotalAmount)
            .ToList();

        return new ExpenseSummaryResponse(totalAmount, totalCount, breakdown);
    }

    private async Task ValidateRecipeExists(Guid? recipeId, CancellationToken cancellationToken)
    {
        if (recipeId.HasValue)
        {
            var recipeExists = await _recipeRepository.GetByIdAsync(recipeId.Value, cancellationToken);
            if (recipeExists is null)
            {
                throw new NotFoundException($"Recipe with ID {recipeId.Value} was not found.");
            }
        }
    }
}
