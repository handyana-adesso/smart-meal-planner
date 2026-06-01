using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeBudgetService.Data;

namespace RecipeBudgetService.Tests.IntegrationTests.Endpoints;

public abstract class BaseEndpointsIntegrationTests : IAsyncLifetime
{
    protected TestWebApplicationFactory Factory = null!;
    protected HttpClient Client = null!;

    public async Task InitializeAsync()
    {
        Factory = new TestWebApplicationFactory();
        Client = Factory.CreateClient();

        // Ensure schema is created once per test
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
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
}
