using System.Runtime.CompilerServices;

namespace RecipeBudgetService.Common.Extensions;

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
}
