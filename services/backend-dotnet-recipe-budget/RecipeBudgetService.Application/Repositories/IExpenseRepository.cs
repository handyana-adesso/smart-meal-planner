using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Application.Repositories;

public interface IExpenseRepository
{
    Task<IList<GroceryExpense>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<GroceryExpense?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<GroceryExpense> CreateAsync(GroceryExpense expense, CancellationToken cancellationToken = default);
    Task<GroceryExpense?> UpdateAsync(GroceryExpense expense, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<List<GroceryExpense>> GetByCategoryAsync(ExpenseCategory category, Guid userId, CancellationToken cancellationToken = default);
}
