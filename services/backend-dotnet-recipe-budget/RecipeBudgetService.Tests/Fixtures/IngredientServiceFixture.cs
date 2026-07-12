using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Tests.Fixtures;

public class IngredientServiceFixture
{
    public Guid UserId { get; } = Guid.NewGuid();

    public Recipe Recipe { get; }

    public IngredientServiceFixture()
    {
        Recipe = new()
        {
            Id = Guid.NewGuid(),
            Name = "Pasta",
            Description = "A delicious pasta",
            Servings = 2,
            CreatedAt = DateTime.Now,
            UserId = UserId,
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
}

