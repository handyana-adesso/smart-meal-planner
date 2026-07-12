using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Application.DTOs;

public record ExpenseCategoryBreakdown(
    ExpenseCategory Category,
    string CategoryName,
    decimal TotalAmount,
    int Count,
    decimal Percentage
);

public record ExpenseSummaryResponse(
    decimal TotalAmount,
    int TotalCount,
    List<ExpenseCategoryBreakdown> Breakdowns
);
