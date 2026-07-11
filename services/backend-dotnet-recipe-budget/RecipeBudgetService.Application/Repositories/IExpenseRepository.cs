using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Application.Repositories;

public interface IExpenseRepository
{
    Task<IList<GroceryExpense>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GroceryExpense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GroceryExpense> CreateAsync(GroceryExpense expense, CancellationToken cancellationToken = default);
    Task<GroceryExpense?> UpdateAsync(GroceryExpense expense, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<GroceryExpense>> GetByCategoryAsync(ExpenseCategory category, CancellationToken cancellationToken = default);
}
