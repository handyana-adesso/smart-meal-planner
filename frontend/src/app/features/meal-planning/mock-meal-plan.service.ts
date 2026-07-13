import { Injectable, signal } from '@angular/core';
import { Observable, delay, of } from 'rxjs';
import { MealPlanEntry } from './meal-plan.model';

// Mock implementation — same Observable-based shape a real HTTP client service
// would expose, so this can be swapped for a real API client once the Java
// Grocery & Meal Plan service exists, without touching any component code.
@Injectable({ providedIn: 'root' })
export class MockMealPlanService {
  private readonly entries = signal<MealPlanEntry[]>([
    { id: crypto.randomUUID(), date: this.todayPlusDays(0), mealType: 'Dinner', recipeName: 'Pasta' },
    { id: crypto.randomUUID(), date: this.todayPlusDays(1), mealType: 'Lunch', recipeName: 'Leftovers' },
  ]);

  getAll(): Observable<MealPlanEntry[]> {
    return of(this.entries()).pipe(delay(150));
  }

  add(entry: Omit<MealPlanEntry, 'id'>): Observable<MealPlanEntry> {
    const created: MealPlanEntry = { ...entry, id: crypto.randomUUID() };
    this.entries.update((current) => [...current, created]);
    return of(created).pipe(delay(150));
  }

  remove(id: string): Observable<void> {
    this.entries.update((current) => current.filter((entry) => entry.id !== id));
    return of(void 0).pipe(delay(150));
  }

  private todayPlusDays(days: number): string {
    const date = new Date();
    date.setDate(date.getDate() + days);
    return date.toISOString().substring(0, 10);
  }
}
