using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RecipeBudgetService.Domain.Exceptions;

namespace RecipeBudgetService.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found."),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation error."),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict occurred."),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized."),
            ArgumentNullException => (StatusCodes.Status400BadRequest, "A required argument was null."),
            ArgumentException => (StatusCodes.Status400BadRequest, "An argument was invalid."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
