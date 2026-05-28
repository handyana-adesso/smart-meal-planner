using RecipeBudgetService.Entities;
using Microsoft.EntityFrameworkCore;

namespace RecipeBudgetService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
}
