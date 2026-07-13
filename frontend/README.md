![Angular](https://img.shields.io/badge/Angular-22-DD0031?logo=angular&logoColor=white) ![Angular Material](https://img.shields.io/badge/Angular%20Material-22-DD0031?logo=angular&logoColor=white) ![TypeScript](https://img.shields.io/badge/TypeScript-6-3178C6?logo=typescript&logoColor=white) ![Vitest](https://img.shields.io/badge/Vitest-4-6E9F18?logo=vitest&logoColor=white) ![Docker](https://img.shields.io/badge/Docker-Latest-2496ED?logo=docker&logoColor=white)

# Smart Meal Planner — Frontend

Angular frontend for the Smart Meal Planner platform. Standalone components,
Angular Material, and signals for state — no NgModules, no NgRx.

## Status

- ✅ **Auth** (login, register) — wired to the .NET API's JWT endpoints
- ✅ **Recipes** — list, create, edit, delete, with dynamic ingredient rows
- ✅ **Expenses** — list with category filter, create/edit, summary breakdown
- ✅ **Monthly report** — month/year picker with per-category breakdown
- 🚧 **Meal planning / Grocery list / Pantry inventory** — mock/hardcoded data for now, since the Java Grocery & Meal Plan API doesn't exist yet. Each of these has its own mock service (`MockMealPlanService`, `MockGroceryListService`, `MockPantryService`) exposing the same `Observable`-based shape a real HTTP client service would, so swapping in the real API later is a one-line change per feature, not a rewrite.

## Project structure

```
src/app/
├── core/
│   ├── auth/          # AuthService, jwtInterceptor, authGuard
│   ├── models/        # TS interfaces mirroring the .NET API's DTOs exactly
│   └── validators/     # passwordComplexityValidator (matches backend rules)
├── features/
│   ├── auth/           # login, register
│   ├── recipes/        # RecipeService + list + form
│   ├── expenses/        # ExpenseService + list + form + summary
│   ├── reports/         # ReportService + monthly report view
│   ├── meal-planning/   # mock data
│   ├── grocery-list/    # mock data
│   └── pantry/          # mock data
└── shared/              # ConfirmDialog, NotificationService, BreakdownList
```

## Auth flow

- `AuthService` holds `accessToken`/`refreshToken` as signals, persisted to `localStorage` so a page reload doesn't force re-login.
- `jwtInterceptor` attaches `Authorization: Bearer <token>` to every request except `/api/auth/*`. On a `401`, it attempts one silent refresh and retries; if that also fails, it logs out and redirects to `/login`.
- `authGuard` protects every route except `/login` and `/register`.

## Getting started

### Prerequisites

- Node.js 22+
- The .NET Recipe & Budget API running (see [../services/backend-dotnet-recipe-budget/README.md](../services/backend-dotnet-recipe-budget/README.md)) — this frontend has no backend of its own.

### Setup

```bash
npm install
npm start          # ng serve, http://localhost:4200
```

The API base URL is configured in `src/environments/environment.development.ts` (defaults to `http://localhost:5019`, matching the .NET service's `dotnet run` HTTP profile).

### Testing

```bash
npm test            # ng test --watch=false (Vitest)
```

### Building

```bash
npm run build        # outputs to dist/frontend/browser
```

## Docker

Built via a multi-stage `Dockerfile` (Node build → nginx serving the static output), wired into `infrastructure/docker/docker-compose.yml` as the `frontend` service on port `4200`.
