using Microsoft.EntityFrameworkCore;
using RecipeBudgetService.Data;
using RecipeBudgetService.Entities;

namespace RecipeBudgetService.Repositories;

public class ExpenseRepository(AppDbContext dbContext) : IExpenseRepository
{
    public async Task<GroceryExpense> CreateAsync(GroceryExpense expense, CancellationToken cancellationToken = default)
    {
        dbContext.GroceryExpenses.Add(expense);
        await dbContext.SaveChangesAsync(cancellationToken);
        return expense;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var expense = await dbContext.GroceryExpenses
            .FindAsync(id, cancellationToken);
        if (expense is null)
        {
            return false;
        }

        dbContext.GroceryExpenses.Remove(expense);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IList<GroceryExpense>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.GroceryExpenses
            .Include(e => e.Recipe)
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<GroceryExpense>> GetByCategoryAsync(ExpenseCategory category, CancellationToken cancellationToken = default)
    {
        return await dbContext.GroceryExpenses
            .Include(e => e.Recipe)
            .Where(e => e.Category == category)
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<GroceryExpense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.GroceryExpenses
            .Include(e => e.Recipe)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<GroceryExpense?> UpdateAsync(GroceryExpense expense, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.GroceryExpenses
            .FindAsync(expense.Id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Description = expense.Description;
        existing.Amount = expense.Amount;
        existing.Category = expense.Category;
        existing.Date = expense.Date;
        existing.RecipeId = expense.RecipeId;

        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }
}
