namespace RecipeBudgetService.Application.DTOs;

public record IngredientResponse(
    Guid Id,
    string Name,
    decimal Quantity,
    string Unit,
    decimal PricePerUnit,
    decimal TotalCost);
