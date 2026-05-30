namespace RecipeBudgetService.DTOs;

public record IngredientRequest(
    string Name,
    decimal Quantity,
    string Unit);
