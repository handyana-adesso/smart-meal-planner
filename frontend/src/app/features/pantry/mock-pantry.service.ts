import { Injectable, signal } from '@angular/core';
import { Observable, delay, of } from 'rxjs';
import { PantryItem } from './pantry-item.model';

// Mock implementation — same Observable-based shape a real HTTP client service
// would expose, so this can be swapped for a real API client once the Java
// Grocery & Meal Plan service exists, without touching any component code.
@Injectable({ providedIn: 'root' })
export class MockPantryService {
  private readonly items = signal<PantryItem[]>([
    { id: crypto.randomUUID(), name: 'Olive oil', quantity: 1, unit: 'bottle', expiryDate: this.todayPlusDays(180) },
    { id: crypto.randomUUID(), name: 'Milk', quantity: 1, unit: 'liter', expiryDate: this.todayPlusDays(2) },
  ]);

  getAll(): Observable<PantryItem[]> {
    return of(this.items()).pipe(delay(150));
  }

  add(item: Omit<PantryItem, 'id'>): Observable<PantryItem> {
    const created: PantryItem = { ...item, id: crypto.randomUUID() };
    this.items.update((current) => [...current, created]);
    return of(created).pipe(delay(150));
  }

  remove(id: string): Observable<void> {
    this.items.update((current) => current.filter((item) => item.id !== id));
    return of(void 0).pipe(delay(150));
  }

  isExpiringSoon(item: PantryItem): boolean {
    if (!item.expiryDate) {
      return false;
    }
    const daysUntilExpiry = (new Date(item.expiryDate).getTime() - Date.now()) / (1000 * 60 * 60 * 24);
    return daysUntilExpiry <= 3;
  }

  private todayPlusDays(days: number): string {
    const date = new Date();
    date.setDate(date.getDate() + days);
    return date.toISOString().substring(0, 10);
  }
}
