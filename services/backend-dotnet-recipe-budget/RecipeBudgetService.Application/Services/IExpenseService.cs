using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Application.Services;

public interface IExpenseService
{
    Task<List<GroceryExpenseResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<GroceryExpenseResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<GroceryExpenseResponse> CreateAsync(GroceryExpenseRequest request, CancellationToken cancellationToken);
    Task<GroceryExpenseResponse> UpdateAsync(Guid id, GroceryExpenseRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<List<GroceryExpenseResponse>> GetByCategoryAsync(ExpenseCategory category, CancellationToken cancellationToken);
    Task<ExpenseSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken);
}
