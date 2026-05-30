using RecipeBudgetService.Entities;
using Microsoft.EntityFrameworkCore;

namespace RecipeBudgetService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the Recipe entity
        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Description).HasMaxLength(500);
            entity.Property(r => r.Servings).IsRequired();
            entity.Property(r => r.EstimatedCost).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(r => r.CreatedAt).IsRequired();
        });

        // Configure the Ingredient entity
        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Name).IsRequired().HasMaxLength(100);
            entity.Property(i => i.Quantity).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(i => i.Unit).IsRequired().HasMaxLength(50);

            // Configure the relationship with Recipe
            entity.HasOne(i => i.Recipe)
                .WithMany(r => r.Ingredients)
                .HasForeignKey(i => i.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
