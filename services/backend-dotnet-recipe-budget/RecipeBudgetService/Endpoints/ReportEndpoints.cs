using Microsoft.AspNetCore.Mvc;
using RecipeBudgetService.Extensions;
using RecipeBudgetService.Filters;
using RecipeBudgetService.Application.DTOs;
using RecipeBudgetService.Application.Services;

namespace RecipeBudgetService.Endpoints;

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reports")
            .WithTags("Reports")
            .RequireAuthorization();

        group.MapGet("/monthly-spending", GetMonthlySpending)
            .WithName("GetMonthlySpendingReport")
            .WithSummary("Get monthly spending report")
            .WithDescription("Returns total spending and a per-category breakdown for the given month and year.")
            .Produces<MonthlySpendingReportResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AddEndpointFilter<ValidationFilter<MonthlySpendingReportRequest>>();
    }

    static async Task<IResult> GetMonthlySpending(
        HttpContext httpContext,
        [AsParameters] MonthlySpendingReportRequest request,
        [FromServices] IReportService reportService,
        CancellationToken cancellationToken)
    {
        var report = await reportService.GetMonthlySpendingReportAsync(request.Month, request.Year, httpContext.GetUserId(), cancellationToken);
        return Results.Ok(report);
    }
}
