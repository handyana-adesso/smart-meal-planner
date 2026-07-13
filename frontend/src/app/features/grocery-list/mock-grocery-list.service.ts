import { Injectable, signal } from '@angular/core';
import { Observable, delay, of } from 'rxjs';
import { GroceryListItem } from './grocery-item.model';

// Mock implementation — same Observable-based shape a real HTTP client service
// would expose, so this can be swapped for a real API client once the Java
// Grocery & Meal Plan service exists, without touching any component code.
@Injectable({ providedIn: 'root' })
export class MockGroceryListService {
  private readonly items = signal<GroceryListItem[]>([
    { id: crypto.randomUUID(), name: 'Spaghetti', quantity: 500, unit: 'g', isChecked: false },
    { id: crypto.randomUUID(), name: 'Eggs', quantity: 12, unit: 'pcs', isChecked: false },
  ]);

  getAll(): Observable<GroceryListItem[]> {
    return of(this.items()).pipe(delay(150));
  }

  add(item: Omit<GroceryListItem, 'id' | 'isChecked'>): Observable<GroceryListItem> {
    const created: GroceryListItem = { ...item, id: crypto.randomUUID(), isChecked: false };
    this.items.update((current) => [...current, created]);
    return of(created).pipe(delay(150));
  }

  toggleChecked(id: string): Observable<void> {
    this.items.update((current) =>
      current.map((item) => (item.id === id ? { ...item, isChecked: !item.isChecked } : item)),
    );
    return of(void 0).pipe(delay(150));
  }

  remove(id: string): Observable<void> {
    this.items.update((current) => current.filter((item) => item.id !== id));
    return of(void 0).pipe(delay(150));
  }
}
