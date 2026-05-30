namespace RecipeBudgetService.DTOs;

public record IngredientResponse(
    Guid Id,
    string Name,
    decimal Quantity,
    string Unit);
