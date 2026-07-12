using RecipeBudgetService.Application.DTOs;

namespace RecipeBudgetService.Application.Services;

public interface IReportService
{
    Task<MonthlySpendingReportResponse> GetMonthlySpendingReportAsync(int month, int year, Guid userId, CancellationToken cancellationToken);
}
