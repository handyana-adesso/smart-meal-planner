using RecipeBudgetService.Entities;

namespace RecipeBudgetService.Tests.Fixtures;

public class IngredientServiceFixture
{
    public Recipe Recipe { get; } = new()
    {
        Id = Guid.NewGuid(),
        Name = "Pasta",
        Description = "A delicious pasta",
        Servings = 2,
        CreatedAt = DateTime.Now,
        Ingredients =
        [
            new Ingredient
            {
                Id = Guid.NewGuid(),
                Name = "Tomato",
                Quantity = 200,
                Unit = "g",
                PricePerUnit = 0.01m
            }
        ]
    };
}
