import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
  },
  {
    path: 'recipes',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./features/recipes/recipe-list/recipe-list').then((m) => m.RecipeList),
      },
      {
        path: 'new',
        loadComponent: () => import('./features/recipes/recipe-form/recipe-form').then((m) => m.RecipeForm),
      },
      {
        path: ':id/edit',
        loadComponent: () => import('./features/recipes/recipe-form/recipe-form').then((m) => m.RecipeForm),
      },
    ],
  },
  {
    path: 'expenses',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./features/expenses/expense-list/expense-list').then((m) => m.ExpenseList),
      },
      {
        path: 'summary',
        loadComponent: () =>
          import('./features/expenses/expense-summary/expense-summary').then((m) => m.ExpenseSummary),
      },
      {
        path: 'new',
        loadComponent: () => import('./features/expenses/expense-form/expense-form').then((m) => m.ExpenseForm),
      },
      {
        path: ':id/edit',
        loadComponent: () => import('./features/expenses/expense-form/expense-form').then((m) => m.ExpenseForm),
      },
    ],
  },
  {
    path: 'reports',
    canActivate: [authGuard],
    loadComponent: () => import('./features/reports/report-view/report-view').then((m) => m.ReportView),
  },
  {
    path: 'meal-planning',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/meal-planning/meal-plan-list/meal-plan-list').then((m) => m.MealPlanList),
  },
  {
    path: 'grocery-list',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/grocery-list/grocery-list-view/grocery-list-view').then((m) => m.GroceryListView),
  },
  {
    path: 'pantry',
    canActivate: [authGuard],
    loadComponent: () => import('./features/pantry/pantry-list/pantry-list').then((m) => m.PantryList),
  },
  { path: '', pathMatch: 'full', redirectTo: 'recipes' },
  { path: '**', redirectTo: 'recipes' },
];
