using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RecipeBudgetService.Data;
using RecipeBudgetService.Entities;
using RecipeBudgetService.Repositories;

namespace RecipeBudgetService.Tests.IntegrationTests.Repositories;

public class ExpenseRepositoryTests
{
    private readonly AppDbContext _dbContext;
    private readonly ExpenseRepository _repository;
    private readonly Recipe _recipe;

    public ExpenseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _repository = new ExpenseRepository(_dbContext);

        // seed a recipe for optional link
        _recipe = new Recipe { Name = "Pasta", Servings = 2 };
        _dbContext.Recipes.Add(_recipe);
        _dbContext.SaveChanges();
        _dbContext.ChangeTracker.Clear();
    }

    [Fact]
    public async Task GetAllAsync_WhenNoExpenses_ShouldReturnEmptyList()
    {
        var result = await _repository.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOrderedByDateDescending()
    {
        // Arrange
        _dbContext.GroceryExpenses.AddRange(
            new GroceryExpense
            {
                Description = "Old expense",
                Amount = 10.00m,
                Category = ExpenseCategory.Groceries,
                Date = DateTime.UtcNow.AddDays(-5)
            },
            new GroceryExpense
            {
                Description = "Recent expense",
                Amount = 20.00m,
                Category = ExpenseCategory.Groceries,
                Date = DateTime.UtcNow
            }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Description.Should().Be("Recent expense");
    }

    [Fact]
    public async Task GetAllAsync_ShouldIncludeRecipe()
    {
        // Arrange
        _dbContext.GroceryExpenses.Add(new GroceryExpense
        {
            Description = "Pasta ingredients",
            Amount = 20.00m,
            Category = ExpenseCategory.Groceries,
            Date = DateTime.UtcNow,
            RecipeId = _recipe.Id
        });
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.First().Recipe.Should().NotBeNull();
        result.First().Recipe!.Name.Should().Be("Pasta");
    }

    [Fact]
    public async Task GetByIdAsync_WhenExpenseExists_ShouldReturnExpense()
    {
        // Arrange
        var expense = new GroceryExpense
        {
            Description = "Weekly groceries",
            Amount = 50.00m,
            Category = ExpenseCategory.Groceries,
            Date = DateTime.UtcNow
        };
        _dbContext.GroceryExpenses.Add(expense);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByIdAsync(expense.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Description.Should().Be("Weekly groceries");
        result.Amount.Should().Be(50.00m);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExpenseDoesNotExist_ShouldReturnNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldSaveAndReturnExpense()
    {
        // Arrange
        var expense = new GroceryExpense
        {
            Description = "Weekly groceries",
            Amount = 50.00m,
            Category = ExpenseCategory.Groceries,
            Date = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(expense);
        _dbContext.ChangeTracker.Clear();

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
        result.Description.Should().Be("Weekly groceries");

        var saved = await _dbContext.GroceryExpenses.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_WithRecipeId_ShouldSaveRecipeLink()
    {
        // Arrange
        var expense = new GroceryExpense
        {
            Description = "Pasta ingredients",
            Amount = 20.00m,
            Category = ExpenseCategory.Groceries,
            Date = DateTime.UtcNow,
            RecipeId = _recipe.Id
        };

        // Act
        var result = await _repository.CreateAsync(expense);
        _dbContext.ChangeTracker.Clear();

        // Assert
        var saved = await _dbContext.GroceryExpenses.FindAsync(result.Id);
        saved!.RecipeId.Should().Be(_recipe.Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenExpenseExists_ShouldUpdateAndReturn()
    {
        // Arrange
        var expense = new GroceryExpense
        {
            Description = "Weekly groceries",
            Amount = 50.00m,
            Category = ExpenseCategory.Groceries,
            Date = DateTime.UtcNow
        };
        _dbContext.GroceryExpenses.Add(expense);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _repository.UpdateAsync(new GroceryExpense
        {
            Id = expense.Id,
            Description = "Updated groceries",
            Amount = 75.00m,
            Category = ExpenseCategory.Household,
            Date = DateTime.UtcNow
        });
        _dbContext.ChangeTracker.Clear();

        // Assert
        result.Should().NotBeNull();
        result!.Description.Should().Be("Updated groceries");
        result.Amount.Should().Be(75.00m);
        result.Category.Should().Be(ExpenseCategory.Household);

        var saved = await _dbContext.GroceryExpenses.FindAsync(expense.Id);
        saved!.Description.Should().Be("Updated groceries");
    }

    [Fact]
    public async Task UpdateAsync_WhenExpenseDoesNotExist_ShouldReturnNull()
    {
        var result = await _repository.UpdateAsync(new GroceryExpense
        {
            Id = Guid.NewGuid(),
            Description = "Ghost",
            Amount = 10.00m,
            Category = ExpenseCategory.Groceries,
            Date = DateTime.UtcNow
        });

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenExpenseExists_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var expense = new GroceryExpense
        {
            Description = "Weekly groceries",
            Amount = 50.00m,
            Category = ExpenseCategory.Groceries,
            Date = DateTime.UtcNow
        };
        _dbContext.GroceryExpenses.Add(expense);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _repository.DeleteAsync(expense.Id);
        _dbContext.ChangeTracker.Clear();

        // Assert
        result.Should().BeTrue();
        var deleted = await _dbContext.GroceryExpenses.FindAsync(expense.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenExpenseDoesNotExist_ShouldReturnFalse()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnOnlyMatchingCategory()
    {
        // Arrange
        _dbContext.GroceryExpenses.AddRange(
            new GroceryExpense
            {
                Description = "Groceries",
                Amount = 50.00m,
                Category = ExpenseCategory.Groceries,
                Date = DateTime.UtcNow
            },
            new GroceryExpense
            {
                Description = "Dinner",
                Amount = 35.00m,
                Category = ExpenseCategory.EatingOut,
                Date = DateTime.UtcNow
            }
        );
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetByCategoryAsync(ExpenseCategory.Groceries);

        // Assert
        result.Should().HaveCount(1);
        result.First().Category.Should().Be(ExpenseCategory.Groceries);
    }
}
