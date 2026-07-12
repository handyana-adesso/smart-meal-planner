using Microsoft.EntityFrameworkCore;
using RecipeBudgetService.Domain.Entities;
using RecipeBudgetService.Application.Repositories;
using RecipeBudgetService.Infrastructure.Data;

namespace RecipeBudgetService.Infrastructure.Repositories;

public class ExpenseRepository(AppDbContext dbContext) : IExpenseRepository
{
    public async Task<GroceryExpense> CreateAsync(GroceryExpense expense, CancellationToken cancellationToken = default)
    {
        dbContext.GroceryExpenses.Add(expense);
        await dbContext.SaveChangesAsync(cancellationToken);
        return expense;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var expense = await dbContext.GroceryExpenses
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, cancellationToken);
        if (expense is null)
        {
            return false;
        }

        dbContext.GroceryExpenses.Remove(expense);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IList<GroceryExpense>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.GroceryExpenses
            .Include(e => e.Recipe)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<GroceryExpense>> GetByCategoryAsync(ExpenseCategory category, Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.GroceryExpenses
            .Include(e => e.Recipe)
            .Where(e => e.Category == category && e.UserId == userId)
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<GroceryExpense?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.GroceryExpenses
            .Include(e => e.Recipe)
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, cancellationToken);
    }

    public async Task<GroceryExpense?> UpdateAsync(GroceryExpense expense, Guid userId, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.GroceryExpenses
            .FirstOrDefaultAsync(e => e.Id == expense.Id && e.UserId == userId, cancellationToken);
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
