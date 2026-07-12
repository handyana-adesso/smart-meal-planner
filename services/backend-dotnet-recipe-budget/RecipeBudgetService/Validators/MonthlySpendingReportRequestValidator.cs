using FluentValidation;
using RecipeBudgetService.Application.DTOs;

namespace RecipeBudgetService.Validators;

public class MonthlySpendingReportRequestValidator : AbstractValidator<MonthlySpendingReportRequest>
{
    public MonthlySpendingReportRequestValidator()
    {
        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");

        RuleFor(x => x.Year)
            .GreaterThanOrEqualTo(2000).WithMessage("Year must be 2000 or later.");
    }
}
