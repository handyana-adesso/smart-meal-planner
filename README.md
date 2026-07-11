# Smart Meal Planner

Smart Meal Planner is a fullstack microservice-based platform for recipes, meal planning, grocery lists, pantry inventory and grocery budget tracking.

## Goals
- Build a useful daily-life application
- Refresh practical .NET and Java Spring Boot skills
- Practice REST APIs, PostgreSQL, authentication and testing
- Practice Docker, CI/CD, Kubernetes and monitoring
- Create real project stories for future technical interviews

---

## Architecture

```mermaid
flowchart TD
  FE(["Frontend\nAngular"])

  subgraph Backend Services
    subgraph .NET[".NET Service\nRecipe & Budget API"]
      NET_EP[Endpoints]
      NET_SV[Services]
      NET_RP[Repositories]
      NET_EP --> NET_SV --> NET_RP
    end

    subgraph JAVA["Java Service\nGrocery & Meal Plan API"]
      JAVA_EP[Endpoints]
      JAVA_SV[Services]
      JAVA_RP[Repositories]
      JAVA_EP --> JAVA_SV --> JAVA_RP
    end
  end

  subgraph Data
    PG1[(PostgreSQL\nRecipe DB)]
    PG2[(PostgreSQL\nGrocery DB)]
  end

  FE -->|REST| NET_EP
  FE -->|REST| JAVA_EP
  NET_RP --> PG1
  JAVA_RP --> PG2
```

> No API Gateway/BFF is used in the first phase. It can be added later as an advanced improvement.

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | ![Angular](https://img.shields.io/badge/Angular-18-DD0031?logo=angular&logoColor=white) |
| .NET Backend | ![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white) ![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10-512BD4?logo=dotnet&logoColor=white) ![EF Core](https://img.shields.io/badge/EF%20Core-10-512BD4?logo=dotnet&logoColor=white) ![FluentValidation](https://img.shields.io/badge/FluentValidation-12-FF6B6B) ![xUnit](https://img.shields.io/badge/xUnit-2.9+-512BD4) |
| Java Backend | ![Spring Boot](https://img.shields.io/badge/Spring%20Boot-3-6DB33F?logo=spring&logoColor=white) ![Spring Security](https://img.shields.io/badge/Spring%20Security-6-6DB33F?logo=spring&logoColor=white) ![JUnit](https://img.shields.io/badge/JUnit-5-25A162?logo=junit5&logoColor=white) |
| Database | ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-316192?logo=postgresql&logoColor=white) |
| Infrastructure | ![Docker](https://img.shields.io/badge/Docker-Latest-2496ED?logo=docker&logoColor=white) ![Docker Compose](https://img.shields.io/badge/Docker%20Compose-Latest-2496ED?logo=docker&logoColor=white) ![GitHub Actions](https://img.shields.io/badge/GitHub%20Actions-Latest-2088FF?logo=github-actions&logoColor=white) ![Kubernetes](https://img.shields.io/badge/Kubernetes-Latest-326CE5?logo=kubernetes&logoColor=white) |
| Testing | ![xUnit](https://img.shields.io/badge/xUnit-2.9+-512BD4) ![JUnit](https://img.shields.io/badge/JUnit-5-25A162?logo=junit5&logoColor=white) ![Cypress](https://img.shields.io/badge/Cypress-13-17202C?logo=cypress&logoColor=white) ![Playwright](https://img.shields.io/badge/Playwright-Latest-2EAD33?logo=playwright&logoColor=white) |

---

## Main Features
- Recipe management
- Ingredient management
- Grocery list management
- Weekly meal planning
- Pantry inventory
- Expiry reminders
- Grocery budget reports
- Receipt upload

---

## Project Structure

```
smart-meal-planner
|-- README.md       <-- you are here
|
|-- services
|   |-- backend-dotnet-recipe-budget
|   |   |-- SERVICE_README.md     <-- .NET Service Documentation (Clean Architecture)
|   |   |-- backend-dotnet-recipe-budget.slnx
|   |   |-- RecipeBudgetService/                 (Presentation Layer)
|   |   |-- RecipeBudgetService.Application/     (Application Layer)
|   |   |-- RecipeBudgetService.Domain/          (Domain Layer)
|   |   |-- RecipeBudgetService.Infrastructure/  (Infrastructure Layer)
|   |   |-- RecipeBudgetService.Tests/
|   |   |-- ...
|   |-- backend-java-grocery-mealplan
|       |-- README.md
|       |-- ...
|
|-- frontend
|   |-- README.md
|   |-- ...
|
|-- docs
|   |-- ...
|
|-- infrastructure
    |-- docker
        |-- docker-compose.yml
        |-- .env.example
```

### .NET Backend Service Architecture

The `.NET Recipe Budget Service` follows **Clean Architecture** principles with clear layer separation:

- **Domain Layer**: Core business entities and domain exceptions (database-agnostic)
- **Application Layer**: Business logic, services, DTOs, and repository interfaces
- **Infrastructure Layer**: Database access, EF Core implementation, and concrete repositories
- **Presentation Layer**: REST API endpoints, middleware, validators, and dependency injection

See [SERVICE_README.md](./services/backend-dotnet-recipe-budget/SERVICE_README.md) for detailed documentation.

---

## Run Instructions

### 1. Clone the repository

```bash
git clone https://github.com/handyana-sa/smart-meal-planner.git
cd smart-meal-planner
```

### 2. Set up environment variables

```bash
cp .env.example .env
```

Edit `.env` with your values:

```env
# .NET Recipe & Budget API
DOTNET_POSTGRES_DB=recipedb
DOTNET_POSTGRES_USER=postgres
DOTNET_POSTGRES_PASSWORD=yourpassword
DOTNET_CONNECTION_STRING=Host=db-recipe;Port=5432;Database=recipedb;Username=postgres;Password=yourpassword
```

### 3. Run everything with Docker Compose

```bash
docker compose up --build
```
| Service | URL |
|---------|-----|
| .NET API | http://localhost:5000 |
| .NET Swagger | http://localhost:5000/swagger |

### 4. Run individual services

See each service's own README for detailed run instructions:

- [.NET Recipe & Budget API](./services/backend-dotnet-recipe-budget/README.md)

---

## Run Tests

```bash
# .NET Tests
cd backend-dotnet-recipe-budget
dotnet test

# Java Tests - TODO

# Frontend Tests - TODO


```

---

## Services

### .NET Recipe & Budget API

Manages recipes, ingredients and estimated costs.
-> [Full documentation](./services/backend-dotnet-recipe-budget/README.md)

---

## Docker & DB Persistence

All docker are **stateless**, data is never stored inside containers. Each service has its own PostgreSQL container with named volume:

```yaml
volumes:
  postgres_recipe_data:       # <-- Recipe & Budget data
  postgres_grocery_data:      # <-- Grocery & Meal Plan data
```

| Command | Effect |
|---------|--------|
| `docker compose down` | Stops containers - data kept ✅ |
| `docker compose down -v` | Stops containers — data deleted ⚠️ |

---

## Status

| Service | Status |
|---------|--------|
| .NET Recipe & Budget API | 🟢 In progress |
| Java Grocery & Meal Plan API | 🔴 Not started |
| Angular Frontend | 🔴 Not started |

---



