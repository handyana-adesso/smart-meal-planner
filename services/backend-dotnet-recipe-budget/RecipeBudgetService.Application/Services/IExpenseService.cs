using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Application.Services;

public interface IExpenseService
{
    Task<List<GroceryExpenseResponse>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<GroceryExpenseResponse> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<GroceryExpenseResponse> CreateAsync(GroceryExpenseRequest request, Guid userId, CancellationToken cancellationToken);
    Task<GroceryExpenseResponse> UpdateAsync(Guid id, GroceryExpenseRequest request, Guid userId, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<List<GroceryExpenseResponse>> GetByCategoryAsync(ExpenseCategory category, Guid userId, CancellationToken cancellationToken);
    Task<ExpenseSummaryResponse> GetSummaryAsync(Guid userId, CancellationToken cancellationToken);
}
