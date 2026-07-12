namespace RecipeBudgetService.Extensions;

public static class HttpContextExtensions
{
    public static Guid GetUserId(this HttpContext httpContext) =>
        Guid.Parse(httpContext.User.FindFirst("sub")!.Value);
}
