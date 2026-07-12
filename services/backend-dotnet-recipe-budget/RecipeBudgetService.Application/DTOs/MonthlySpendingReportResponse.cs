namespace RecipeBudgetService.Application.DTOs;

public record MonthlySpendingReportResponse(
    int Month,
    int Year,
    decimal TotalAmount,
    int TotalCount,
    List<ExpenseCategoryBreakdown> Breakdowns
);
