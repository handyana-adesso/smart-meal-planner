using FluentValidation;
using RecipeBudgetService.DTOs;

namespace RecipeBudgetService.Common.Validators;

public class IngredientRequestValidator : AbstractValidator<IngredientRequest>
{
    public IngredientRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ingredient name is required.")
            .MaximumLength(100).WithMessage("Ingredient name must not exceed 100 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must be a non-negative number.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required.")
            .MaximumLength(50).WithMessage("Unit must not exceed 50 characters.");
    }
}
