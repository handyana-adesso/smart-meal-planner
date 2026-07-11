using System.Runtime.CompilerServices;

namespace RecipeBudgetService.Application.Extensions;

public static class GuardExtensions
{
    public static void ThrowIfGuidEmpty(
        Guid value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Guid cannot be empty", paramName);
        }
    }

    public static void ThrowIfDecimalNegative(
        decimal value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < 0)
        {
            throw new ArgumentException("Decimal value cannot be negative", paramName);
        }
    }
}
