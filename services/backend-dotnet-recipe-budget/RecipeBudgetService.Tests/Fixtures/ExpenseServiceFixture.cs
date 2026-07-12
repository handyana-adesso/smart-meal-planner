using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Tests.Fixtures;

public class ExpenseServiceFixture
{
    public Guid UserId { get; } = Guid.NewGuid();

    public Recipe Recipe { get; }

    public List<GroceryExpense> Expenses { get; set; }

    public ExpenseServiceFixture()
    {
        Recipe = new()
        {
            Id = Guid.NewGuid(),
            Name = "Pasta",
            Description = "A delicious pasta",
            Servings = 2,
            CreatedAt = DateTime.Now,
            UserId = UserId
        };

        Expenses = new List<GroceryExpense>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Description = "Weekly groceries",
                Amount = 50.00m,
                Category = ExpenseCategory.Groceries,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UserId = UserId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Description = "Dinner out",
                Amount = 35.00m,
                Category = ExpenseCategory.EatingOut,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UserId = UserId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Description = "Cleaning supplies",
                Amount = 20.00m,
                Category = ExpenseCategory.Household,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                RecipeId = null,
                UserId = UserId
            }
        };
    }
}

