import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ExpenseCategory,
  ExpenseSummaryResponse,
  GroceryExpenseRequest,
  GroceryExpenseResponse,
} from '../../core/models/expense.model';

@Injectable({ providedIn: 'root' })
export class ExpenseService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/expenses`;

  getAll(): Observable<GroceryExpenseResponse[]> {
    return this.http.get<GroceryExpenseResponse[]>(this.baseUrl);
  }

  getById(id: string): Observable<GroceryExpenseResponse> {
    return this.http.get<GroceryExpenseResponse>(`${this.baseUrl}/${id}`);
  }

  getSummary(): Observable<ExpenseSummaryResponse> {
    return this.http.get<ExpenseSummaryResponse>(`${this.baseUrl}/summary`);
  }

  getByCategory(category: ExpenseCategory): Observable<GroceryExpenseResponse[]> {
    return this.http.get<GroceryExpenseResponse[]>(`${this.baseUrl}/category/${category}`);
  }

  create(request: GroceryExpenseRequest): Observable<GroceryExpenseResponse> {
    return this.http.post<GroceryExpenseResponse>(this.baseUrl, request);
  }

  update(id: string, request: GroceryExpenseRequest): Observable<GroceryExpenseResponse> {
    return this.http.put<GroceryExpenseResponse>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
