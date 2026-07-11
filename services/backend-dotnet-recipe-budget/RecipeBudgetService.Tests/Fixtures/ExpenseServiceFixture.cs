using RecipeBudgetService.Domain.Entities;

namespace RecipeBudgetService.Tests.Fixtures;

public class ExpenseServiceFixture
{
    public Recipe Recipe { get; } = new()
    {
        Id = Guid.NewGuid(),
        Name = "Pasta",
        Description = "A delicious pasta",
        Servings = 2,
        CreatedAt = DateTime.Now
    };

    public List<GroceryExpense> Expenses { get; set; }

    public ExpenseServiceFixture()
    {
        Expenses = new List<GroceryExpense>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Description = "Weekly groceries",
                Amount = 50.00m,
                Category = ExpenseCategory.Groceries,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Description = "Dinner out",
                Amount = 35.00m,
                Category = ExpenseCategory.EatingOut,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Description = "Cleaning supplies",
                Amount = 20.00m,
                Category = ExpenseCategory.Household,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                RecipeId = null
            }
        };
    }
}

