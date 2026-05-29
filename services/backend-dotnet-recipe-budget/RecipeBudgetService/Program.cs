using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RecipeBudgetService.Common.Validators;
using RecipeBudgetService.Data;
using RecipeBudgetService.Endpoints;
using RecipeBudgetService.Repositories;
using RecipeBudgetService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
   options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IRecipeService, RecipeService>();

// Registers all validators in the assembly automatically
builder.Services.AddValidatorsFromAssemblyContaining<RecipeRequestValidator>();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Map health check endpoint
app.MapHealthChecks("/health");

app.MapRecipeEndpoints();

app.Run();
