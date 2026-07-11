using FluentValidation;
using RecipeBudgetService.Application.DTOs;

namespace RecipeBudgetService.Validators;

public class IngredientRequestValidator : AbstractValidator<IngredientRequest>
{
    public IngredientRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ingredient name is required.")
            .MaximumLength(100).WithMessage("Ingredient name must not exceed 100 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required.")
            .MaximumLength(50).WithMessage("Unit must not exceed 50 characters.");

        RuleFor(x => x.PricePerUnit)
            .GreaterThanOrEqualTo(0).WithMessage("Price per unit must be a non-negative number.");
    }
}
