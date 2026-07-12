using Microsoft.AspNetCore.Mvc;
using RecipeBudgetService.Filters;
using RecipeBudgetService.Application.DTOs.Auth;
using RecipeBudgetService.Application.Services;

namespace RecipeBudgetService.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth")
            .AllowAnonymous();

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register a new user")
            .WithDescription("Creates a new user account and returns access and refresh tokens.")
            .Produces<AuthResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .AddEndpointFilter<ValidationFilter<RegisterRequest>>();

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Log in")
            .WithDescription("Validates credentials and returns access and refresh tokens.")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .AddEndpointFilter<ValidationFilter<LoginRequest>>();

        group.MapPost("/refresh", Refresh)
            .WithName("Refresh")
            .WithSummary("Refresh tokens")
            .WithDescription("Exchanges a valid refresh token for a new access and refresh token pair.")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Log out")
            .WithDescription("Revokes the given refresh token.")
            .Produces(StatusCodes.Status204NoContent);
    }

    static async Task<IResult> Register([FromServices] IAuthService authService, RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);
        return Results.Created("/api/auth/register", result);
    }

    static async Task<IResult> Login([FromServices] IAuthService authService, LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        return Results.Ok(result);
    }

    static async Task<IResult> Refresh([FromServices] IAuthService authService, RefreshTokenRequest request)
    {
        var result = await authService.RefreshAsync(request.RefreshToken);
        return Results.Ok(result);
    }

    static async Task<IResult> Logout([FromServices] IAuthService authService, RefreshTokenRequest request)
    {
        await authService.LogoutAsync(request.RefreshToken);
        return Results.NoContent();
    }
}
