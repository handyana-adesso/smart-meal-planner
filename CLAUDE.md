# CLAUDE.md — Smart Meal Planner

This file documents the project plan, architecture decisions, and best practices for Claude to follow when working on this codebase.

---

## Project Overview

Smart Meal Planner is a fullstack microservice-based platform for recipes, meal planning, grocery lists, pantry inventory and grocery budget tracking.

### Three main components

| Component | Technology | Status |
|---|---|---|
| .NET Recipe & Budget API | ASP.NET Core Minimal API, .NET 10 | 🟢 In progress |
| Java Grocery & Meal Plan API | Spring Boot, Spring Security | 🔴 Not started |
| Angular Frontend | Angular | 🔴 Not started |

---

## Roadmap

### Week 1 — .NET Recipe & Budget API ✅
- [x] Project setup with Docker + PostgreSQL
- [x] Recipe CRUD with ingredients
- [x] Estimated cost calculation (computed, never stored)
- [x] Grocery expense tracking with categories
- [x] Expense summary endpoint
- [x] Monthly spending report
- [x] Unit + integration tests
- [x] README + architecture diagrams
- [ ] Authentication (JWT) — deferred to Week 4

### Week 2 — Java Grocery & Meal Plan API ❌
- [ ] Spring Boot project setup with Docker + PostgreSQL
- [ ] Grocery list management
- [ ] Weekly meal planning
- [ ] Pantry inventory
- [ ] Expiry reminders
- [ ] JUnit unit + integration tests
- [ ] README + API documentation

### Week 3 — Angular Frontend ❌
- [ ] Angular project setup
- [ ] Recipe management UI
- [ ] Expense tracking UI
- [ ] Monthly report UI
- [ ] Meal planning UI
- [ ] Grocery list UI

### Week 4 — Authentication + Integration ❌
- [ ] JWT authentication in .NET service
- [ ] Spring Security + JWT in Java service
- [ ] Auth UI in Angular
- [ ] User-scoped data (recipes per user, expenses per user)

### Week 5 — CI/CD ❌
- [ ] GitHub Actions pipeline for .NET
- [ ] GitHub Actions pipeline for Java
- [ ] GitHub Actions pipeline for Angular
- [ ] Jenkins pipeline

### Week 6 — Kubernetes + Monitoring ❌
- [ ] Kubernetes deployment manifests
- [ ] Monitoring setup
- [ ] E2E tests with Cypress / Playwright

---

## Architecture

### Overall system

```
Frontend (Angular)
    ↓ REST          ↓ REST
.NET API        Java API
    ↓               ↓
PostgreSQL      PostgreSQL
(Recipe DB)     (Grocery DB)
```

No API Gateway in phase 1 — can be added later.

### .NET service layers

```
HTTP Request
     ↓
ValidationFilter      — request shape validation (FluentValidation)
     ↓
GlobalExceptionHandler — catches all unhandled exceptions
     ↓
Endpoint              — HTTP only, no business logic
     ↓
Service               — all business logic lives here
     ↓
Repository            — data access only, EF Core queries
     ↓
PostgreSQL
```

### Docker — containers are stateless

- API containers contain **code only** — no data
- All data lives in PostgreSQL with a **named volume**
- `docker compose down` keeps data, `docker compose down -v` deletes it

---

## .NET Service — Best Practices

### Project structure

```
backend-dotnet-recipe-budget/
├── Common/
│   ├── Exceptions/         # NotFoundException, ConflictException
│   ├── Filters/            # ValidationFilter<T>
│   ├── Guards/             # Guard.ThrowIfEmpty, ThrowIfNullOrWhiteSpace
│   ├── Mappings/           # Entity ↔ DTO extension methods
│   └── Middleware/         # GlobalExceptionHandler
├── Data/
│   └── AppDbContext.cs     # DbSets only, no OnModelCreating unless needed
├── DTOs/                   # API contracts — never expose domain models
├── Endpoints/              # Thin — HTTP only, call service, return result
├── Models/                 # Domain entities with data annotations
├── Repositories/           # IRepository + Repository — data access only
├── Services/               # IService + Service — all business logic
└── Validators/             # AbstractValidator<T> per request DTO
```

### Layer responsibilities

**Endpoint** — HTTP concerns only:
- Parse route params, query strings, body
- Call service
- Return correct HTTP status code
- Never contain business logic or database queries

**Service** — Business logic only:
- Guard against invalid input
- Enforce business rules (no duplicate names, valid recipe links)
- Orchestrate repositories
- Map entities to response DTOs
- Never touch `HttpContext` or return `IResult`

**Repository** — Data access only:
- EF Core queries
- Include navigation properties
- Return raw entities (never DTOs)
- Never contain business rules or guards

### API contract rules

- Use **nouns not verbs** in URLs: `/api/recipes` not `/api/getRecipes`
- Use **plural nouns**: `/api/recipes` not `/api/recipe`
- Nest related resources max 2 levels: `/api/recipes/{id}/ingredients`
- Always return **consistent error shape** using `ProblemDetails` (RFC 7807)
- `DELETE` always returns `204` regardless of whether item existed (security)
- Never expose domain models directly — always map to DTOs

### DTOs

- `RecipeRequest` — what the consumer sends (API contract)
- `RecipeResponse` — what the consumer receives (API contract)
- Domain models (`Recipe`, `Ingredient`) — internal only, never serialized directly
- Mapping via extension methods in `Common/Mappings/`
- Computed values (e.g. `EstimatedCost`) live in mappings, never stored in DB

### Validation

Two types of validation — keep them separate:

| Type | Where | Tool | Returns |
|---|---|---|---|
| Request shape | `ValidationFilter<T>` | FluentValidation | `400 Bad Request` |
| Business rules | `Service` | Guard + custom exceptions | `409 Conflict`, `404 Not Found` |

### Error handling

- `GlobalExceptionHandler` catches all unhandled exceptions
- Custom exceptions: `NotFoundException` → `404`, `ConflictException` → `409`
- All errors return `ProblemDetails` format
- Endpoints never use try/catch — only happy path

### EF Core

- Use **data annotations** on entities for simple constraints
- Use `OnModelCreating` only for relationships and things annotations can't do
- Always use `Include()` for navigation properties — no lazy loading
- Never store computed values — derive them from existing data
- `CreatedAt` is always set by server, never by client

### Testing strategy

```
Unit tests      → Service layer     → mock IRepository
Integration     → Repository layer  → SQLite in-memory
Integration     → Endpoint layer    → WebApplicationFactory + SQLite
```

- Unit test classes that have **real logic** (services)
- Integration test classes that have **wiring** (repositories, endpoints)
- Never mock the class you are testing
- Use `IClassFixture` for shared read-only test data
- Use constructor for per-test setup (xUnit creates new instance per test)
- Use `IAsyncLifetime.InitializeAsync` for async setup
- Always call `await base.InitializeAsync()` first in overrides

### Test naming convention

```
MethodName_WhenCondition_ShouldExpectedResult
```

Examples:
- `CreateAsync_WhenNameAlreadyExists_ShouldThrowConflictException`
- `GET_ApiRecipes_WhenNoRecipes_ShouldReturn200WithEmptyList`
- `DeleteAsync_WhenRecipeDoesNotExist_ShouldReturnFalse`

### Security

- `DELETE` endpoints always return `204` — never reveal if resource existed
- Credentials never in source code — use `.env` file (gitignored)
- `.env.example` committed as template with dummy values
- `appsettings.Development.json` gitignored
- Fine-grained PAT tokens scoped to specific repos

### Architecture principles

- **YAGNI** — only build what you actually need right now
- **Small vertical foundation** — one complete working slice before expanding
- **Business logic in service** — never in endpoints or repositories
- **Stateless containers** — all data in PostgreSQL with named volumes
- **No stored computed values** — derive from existing data

---

## Java Service — Best Practices (Week 2)

To be documented when implementation begins. Will follow similar layered architecture:

```
Controller → Service → Repository → PostgreSQL
```

Key differences from .NET:
- Use `@RestController` instead of minimal API endpoints
- Use Spring Data JPA instead of EF Core
- Use `@Valid` + `BindingResult` or `@ExceptionHandler` for validation
- Use JUnit 5 + Mockito for unit tests
- Use `@SpringBootTest` for integration tests

---

## Angular Frontend — Best Practices (Week 3)

To be documented when implementation begins.

---

## Git Conventions

### Branch naming

```
feature/recipe-crud
feature/expense-tracking
fix/migration-error
chore/update-readme
```

### Commit messages

```
feat: add grocery expense tracking
fix: correct cascade delete for ingredients
chore: update README with run instructions
test: add integration tests for expense endpoints
refactor: extract mapping to separate class
```

### What NOT to commit

```gitignore
.env
.env.*
!.env.example
**/appsettings.Development.json
**/appsettings.Local.json
backend-dotnet/**/bin/
backend-dotnet/**/obj/
backend-java/target/
frontend/node_modules/
frontend/dist/
```

---

## Docker Conventions

### Run commands

```bash
# start all services
docker compose up --build

# start only database
docker compose up db -d

# stop — keep data
docker compose down

# stop — delete data (careful!)
docker compose down -v
```

### Environment variables

All credentials go in `.env` next to `docker-compose.yml`:

```env
POSTGRES_DB=recipedb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=yourpassword
CONNECTION_STRING=Host=db;Port=5432;Database=recipedb;Username=postgres;Password=yourpassword
```

---

## Domain Model

### Current entities (.NET service)

```
Recipe
├── Id (Guid)
├── Name (string, required, max 100)
├── Servings (int, min 1)
├── CreatedAt (DateTime, server-set)
└── Ingredients []
    ├── Id (Guid)
    ├── Name (string, required, max 100)
    ├── Quantity (decimal, > 0)
    ├── Unit (string, required, max 50)
    ├── PricePerUnit (decimal, >= 0)
    └── RecipeId (FK)

GroceryExpense
├── Id (Guid)
├── Description (string, required, max 200)
├── Amount (decimal, > 0)
├── Category (enum: Groceries, Household, EatingOut)
├── Date (DateTime)
├── CreatedAt (DateTime, server-set)
├── RecipeId? (optional FK to Recipe)
└── Recipe? (navigation)
```

### Computed values (never stored)

```
EstimatedCost = Ingredients.Sum(i => i.Quantity * i.PricePerUnit)
TotalCost (per ingredient) = Quantity * PricePerUnit
ExpenseSummary = aggregated from GroceryExpense records
MonthlyReport = filtered + grouped GroceryExpense records
```

---

## API Endpoints Reference

### .NET Recipe & Budget API

#### Recipes

| Method | Endpoint | Body | Response |
|---|---|---|---|
| `GET` | `/api/recipes` | — | `200` + list |
| `GET` | `/api/recipes/{id}` | — | `200`, `404` |
| `POST` | `/api/recipes` | `RecipeRequest` | `201`, `400`, `409` |
| `PUT` | `/api/recipes/{id}` | `RecipeRequest` | `200`, `400`, `404`, `409` |
| `DELETE` | `/api/recipes/{id}` | — | `204` |

#### Ingredients

| Method | Endpoint | Body | Response |
|---|---|---|---|
| `POST` | `/api/recipes/{id}/ingredients` | `IngredientRequest` | `201`, `400`, `404` |
| `DELETE` | `/api/recipes/{id}/ingredients/{iId}` | — | `204` |

#### Expenses

| Method | Endpoint | Body | Response |
|---|---|---|---|
| `GET` | `/api/expenses` | — | `200` + list |
| `GET` | `/api/expenses/{id}` | — | `200`, `404` |
| `GET` | `/api/expenses/summary` | — | `200` + summary |
| `GET` | `/api/expenses/category/{category}` | — | `200` + list |
| `POST` | `/api/expenses` | `GroceryExpenseRequest` | `201`, `400`, `404` |
| `PUT` | `/api/expenses/{id}` | `GroceryExpenseRequest` | `200`, `400`, `404` |
| `DELETE` | `/api/expenses/{id}` | — | `204` |

#### Reports

| Method | Endpoint | Params | Response |
|---|---|---|---|
| `GET` | `/api/reports/monthly-spending` | `?month=5&year=2026` | `200`, `400` |

#### Health

| Method | Endpoint | Response |
|---|---|---|
| `GET` | `/health` | `200 Healthy` |
