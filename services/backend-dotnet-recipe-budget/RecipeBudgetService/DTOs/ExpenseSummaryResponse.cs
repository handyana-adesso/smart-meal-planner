using RecipeBudgetService.Entities;

namespace RecipeBudgetService.DTOs;

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
