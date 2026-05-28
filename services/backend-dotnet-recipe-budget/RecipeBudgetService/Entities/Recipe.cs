namespace RecipeBudgetService.Entities;

public class Recipe
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Servings { get; set; }
    public decimal EstimatedCost { get; set; }
    public DateTime CreatedAt { get; set; }
}
