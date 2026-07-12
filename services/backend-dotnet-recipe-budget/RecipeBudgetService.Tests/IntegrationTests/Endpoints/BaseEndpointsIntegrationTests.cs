using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeBudgetService.Application.DTOs.Auth;
using RecipeBudgetService.Infrastructure.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RecipeBudgetService.Tests.IntegrationTests.Endpoints;

public abstract class BaseEndpointsIntegrationTests : IAsyncLifetime
{
    protected TestWebApplicationFactory Factory = null!;
    protected HttpClient Client = null!;
    protected Guid UserId;

    public virtual async Task InitializeAsync()
    {
        Factory = new TestWebApplicationFactory();
        Client = Factory.CreateClient();

        // Ensure schema is created once per test
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        // Every test runs as an authenticated user by default — Client is pre-authenticated.
        var (accessToken, userId) = await RegisterAndLoginAsync();
        UserId = userId;
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }

    // Shared helper to seed data directly into db
    protected async Task<T> SeedAsync<T>(Func<AppDbContext, Task<T>> seeder)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await seeder(db);
    }

    // Registers a new user and returns their access token and user id.
    protected async Task<(string AccessToken, Guid UserId)> RegisterAndLoginAsync(string? email = null)
    {
        email ??= $"{Guid.NewGuid()}@example.com";

        var response = await Factory.CreateClient()
            .PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "SecurePass1"));
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        var userId = await SeedAsync(async db =>
        {
            var user = await db.Users.FirstAsync(u => u.Email == email);
            return user.Id;
        });

        return (body!.AccessToken, userId);
    }

    // Returns a fresh HttpClient authenticated as a different (or the given) user.
    protected async Task<HttpClient> GetAuthenticatedClientAsync(string? email = null)
    {
        var (accessToken, _) = await RegisterAndLoginAsync(email);
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }
}
