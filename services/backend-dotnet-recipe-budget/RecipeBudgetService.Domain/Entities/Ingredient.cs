namespace RecipeBudgetService.Domain.Entities;

public class Ingredient
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal? PricePerUnit { get; set; }
    public Guid RecipeId { get; set; }
    public Recipe? Recipe { get; set; }
}
