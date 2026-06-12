using RecipeBudgetService.Entities;
using Microsoft.EntityFrameworkCore;

namespace RecipeBudgetService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<GroceryExpense> GroceryExpenses => Set<GroceryExpense>();
}
