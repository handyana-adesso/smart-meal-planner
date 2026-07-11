namespace RecipeBudgetService.Application.DTOs;

public record IngredientRequest(
    string Name,
    decimal Quantity,
    string? Unit = null,
    decimal? PricePerUnit = null);
