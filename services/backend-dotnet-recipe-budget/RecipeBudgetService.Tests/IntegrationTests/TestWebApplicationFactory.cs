using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeBudgetService.Infrastructure.Data;

namespace RecipeBudgetService.Tests.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;
    private readonly string _dbName = Guid.NewGuid().ToString();

    public TestWebApplicationFactory()
    {
        _connection = new($"DataSource={_dbName};Mode=Memory;Cache=Shared");
        _connection.Open();

        // Ensure JWT config is available before Program.cs reads it, regardless of
        // how WebApplicationFactory/HostFactoryResolver times config source loading.
        Environment.SetEnvironmentVariable("JWT_SECRET", "test-only-secret-key-minimum-32-characters-long");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "smart-meal-planner-test");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "smart-meal-planner-client-test");
        Environment.SetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRY_MINUTES", "15");
        Environment.SetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRY_DAYS", "7");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureTestServices(services =>
        {
            // remove existing DbContext registrations
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(AppDbContext) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
            ).ToList();

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            // use fixed db name — same database across all requests in this test
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}

