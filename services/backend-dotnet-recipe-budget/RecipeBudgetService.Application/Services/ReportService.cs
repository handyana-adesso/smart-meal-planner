using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Application.Extensions;
using RecipeBudgetService.Application.Repositories;

namespace RecipeBudgetService.Application.Services;

public class ReportService(IExpenseRepository expenseRepository) : IReportService
{
    private readonly IExpenseRepository _expenseRepository = expenseRepository
        ?? throw new ArgumentNullException(nameof(expenseRepository));

    public async Task<MonthlySpendingReportResponse> GetMonthlySpendingReportAsync(int month, int year, Guid userId, CancellationToken cancellationToken)
    {
        GuardExtensions.ThrowIfGuidEmpty(userId);

        if (month is < 1 or > 12)
        {
            throw new ArgumentException("Month must be between 1 and 12.", nameof(month));
        }

        if (year < 2000)
        {
            throw new ArgumentException("Year must be 2000 or later.", nameof(year));
        }

        var expenses = await _expenseRepository.GetAllAsync(userId, cancellationToken);
        var monthlyExpenses = expenses
            .Where(e => e.Date.Month == month && e.Date.Year == year)
            .ToList();

        if (monthlyExpenses.Count == 0)
        {
            return new MonthlySpendingReportResponse(month, year, 0, 0, []);
        }

        var totalAmount = monthlyExpenses.Sum(e => e.Amount);
        var totalCount = monthlyExpenses.Count;

        var breakdown = monthlyExpenses
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

        return new MonthlySpendingReportResponse(month, year, totalAmount, totalCount, breakdown);
    }
}
