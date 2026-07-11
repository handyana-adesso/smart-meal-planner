using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RecipeBudgetService.Middleware;
using RecipeBudgetService.Validators;
using RecipeBudgetService.Endpoints;
using RecipeBudgetService.Infrastructure.Data;
using RecipeBudgetService.Application.Repositories;
using RecipeBudgetService.Application.Services;
using RecipeBudgetService.Infrastructure.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}    

builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IIngredientService, IngredientService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();

// Registers all validators in the assembly automatically
builder.Services.AddValidatorsFromAssemblyContaining<RecipeRequestValidator>();

// Add health checks
builder.Services.AddHealthChecks();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// only migrate in non-test environments
if (!app.Environment.IsEnvironment("Test"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Map health check endpoint
app.MapHealthChecks("/health");

app.MapRecipeEndpoints();
app.MapIngredientEndpoints();
app.MapGroceryExpenseEndpoints();

app.Run();
