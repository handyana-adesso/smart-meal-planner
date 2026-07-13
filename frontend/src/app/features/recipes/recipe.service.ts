import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { IngredientRequest, IngredientResponse, RecipeRequest, RecipeResponse } from '../../core/models/recipe.model';

@Injectable({ providedIn: 'root' })
export class RecipeService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/recipes`;

  getAll(): Observable<RecipeResponse[]> {
    return this.http.get<RecipeResponse[]>(this.baseUrl);
  }

  getById(id: string): Observable<RecipeResponse> {
    return this.http.get<RecipeResponse>(`${this.baseUrl}/${id}`);
  }

  create(request: RecipeRequest): Observable<RecipeResponse> {
    return this.http.post<RecipeResponse>(this.baseUrl, request);
  }

  update(id: string, request: RecipeRequest): Observable<RecipeResponse> {
    return this.http.put<RecipeResponse>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  addIngredient(recipeId: string, request: IngredientRequest): Observable<IngredientResponse> {
    return this.http.post<IngredientResponse>(`${this.baseUrl}/${recipeId}/ingredients`, request);
  }

  deleteIngredient(recipeId: string, ingredientId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${recipeId}/ingredients/${ingredientId}`);
  }
}
