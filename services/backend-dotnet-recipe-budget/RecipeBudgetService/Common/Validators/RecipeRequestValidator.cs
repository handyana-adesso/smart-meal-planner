using FluentValidation;
using RecipeBudgetService.DTOs;

namespace RecipeBudgetService.Common.Validators;

public class RecipeRequestValidator : AbstractValidator<RecipeRequest>
{
    public RecipeRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage("Recipe name is required.")
            .MaximumLength(100).WithMessage("Recipe name must not exceed 100 characters.");
        RuleFor(r => r.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
        RuleFor(r => r.Servings)
            .GreaterThan(0).WithMessage("Servings must be at least 1.");
        RuleForEach(r => r.Ingredients)
            .SetValidator(new IngredientRequestValidator());
    }
}
