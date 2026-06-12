using FluentValidation;
using RecipeBudgetService.DTOs;

namespace RecipeBudgetService.Common.Validators;

public class GroceryExpenseRequestValidator : AbstractValidator<GroceryExpenseRequest>
{
    public GroceryExpenseRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.")
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Category)
            .IsInEnum()
            .WithMessage("Invalid expense category.");

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.Date.HasValue)
            .WithMessage("Date cannot be in the future.");
    }
}
