# Recipe Budget Service (.NET)

A comprehensive RESTful API service for managing recipes, ingredients, and grocery expenses with budget tracking. Built using **Clean Architecture** principles with separated concerns across Domain, Application, Infrastructure, and Presentation layers.

## 🏗️ Architecture Overview

This project follows **Clean Architecture** with four main layers:

- **Domain Layer** (RecipeBudgetService.Domain): Core business entities and domain exceptions
- **Application Layer** (RecipeBudgetService.Application): Business logic, services, DTOs, and repository interfaces
- **Infrastructure Layer** (RecipeBudgetService.Infrastructure): Database access and EF Core implementations
- **Presentation Layer** (RecipeBudgetService): API endpoints, middleware, validators, and dependency injection

## 🚀 Tech Stack

| Component | Technology |
|-----------|-----------|
| **Framework** | ![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white) |
| **Language** | ![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp&logoColor=white) |
| **Database** | ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-316192?logo=postgresql&logoColor=white) |
| **ORM** | ![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?logo=dotnet&logoColor=white) |
| **Validation** | ![FluentValidation](https://img.shields.io/badge/FluentValidation-12.1-FF6B6B) |
| **API Docs** | ![OpenAPI](https://img.shields.io/badge/OpenAPI-3.1-6BA539?logo=openapis&logoColor=white) |
| **Container** | ![Docker](https://img.shields.io/badge/Docker-Latest-2496ED?logo=docker&logoColor=white) |
| **Testing** | ![xUnit](https://img.shields.io/badge/xUnit-2.9+-512BD4) |

## 📁 Project Structure

**Domain Layer** (Pure business logic, no dependencies)
- Entities: Recipe, Ingredient, GroceryExpense
- Exceptions: NotFoundException, ConflictException, ValidationException

**Application Layer** (Business logic orchestration)
- Services: RecipeService, IngredientService, ExpenseService
- DTOs: Request/Response objects for API contracts
- Repositories: Interfaces for data access
- Mappers: Entity to DTO conversions

**Infrastructure Layer** (Data access implementation)
- AppDbContext: EF Core database context
- Repositories: Concrete repository implementations
- Migrations: Database schema versioning

**Presentation Layer** (API endpoints)
- Endpoints: REST API route definitions
- Middleware: Exception handling, logging
- Validators: Input validation rules
- Program.cs: Dependency injection setup

## 🛠️ Features

- ✅ Recipe management (CRUD operations)
- ✅ Ingredient management with pricing
- ✅ Grocery expense tracking and categorization
- ✅ Expense summary and analytics
- ✅ RESTful API with OpenAPI documentation
- ✅ Comprehensive error handling
- ✅ Input validation with FluentValidation

## 📚 API Endpoints

**Recipes**
- GET /api/recipes - List all recipes
- POST /api/recipes - Create recipe
- GET /api/recipes/{id} - Get recipe by ID
- PUT /api/recipes/{id} - Update recipe
- DELETE /api/recipes/{id} - Delete recipe

**Expenses**
- GET /api/expenses - List expenses
- POST /api/expenses - Create expense
- GET /api/expenses/{id} - Get expense by ID
- PUT /api/expenses/{id} - Update expense
- DELETE /api/expenses/{id} - Delete expense
- GET /api/expenses/summary - Get expense summary

## 🚀 Getting Started

### Prerequisites
- .NET 10 SDK
- PostgreSQL 16+
- Docker (optional)

### Setup
1. Configure database connection in appsettings.json
2. Run: dotnet restore
3. Run: dotnet ef database update
4. Run: dotnet run
5. Visit: https://localhost:5001/scalar

## 🧪 Testing

dotnet test

## 📖 Clean Architecture Principles

- Dependencies flow inward (toward Domain)
- Domain layer has zero external dependencies
- Business logic is independent of frameworks
- Easy to test and maintain
- Easy to swap implementations

## 🤝 Contributing

Contributions are welcome! Please fork, create a feature branch, and submit a pull request.
