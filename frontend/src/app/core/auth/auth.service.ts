import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  AuthResponse,
  LoginRequest,
  RefreshTokenRequest,
  RegisterRequest,
} from '../models/auth.model';

const ACCESS_TOKEN_KEY = 'smp_access_token';
const REFRESH_TOKEN_KEY = 'smp_refresh_token';

interface DecodedUser {
  userId: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/auth`;

  private readonly accessTokenSignal = signal<string | null>(
    localStorage.getItem(ACCESS_TOKEN_KEY),
  );
  private readonly refreshTokenSignal = signal<string | null>(
    localStorage.getItem(REFRESH_TOKEN_KEY),
  );

  readonly isAuthenticated = computed(() => !!this.accessTokenSignal());
  readonly currentUser = computed(() => this.decodeUser(this.accessTokenSignal()));

  accessToken(): string | null {
    return this.accessTokenSignal();
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/register`, request)
      .pipe(tap((res) => this.setTokens(res)));
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/login`, request)
      .pipe(tap((res) => this.setTokens(res)));
  }

  refresh(): Observable<AuthResponse> {
    const refreshToken = this.refreshTokenSignal();
    const request: RefreshTokenRequest = { refreshToken: refreshToken ?? '' };
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/refresh`, request)
      .pipe(tap((res) => this.setTokens(res)));
  }

  logout(): Observable<void> {
    const refreshToken = this.refreshTokenSignal();
    const request$ = refreshToken
      ? this.http.post<void>(`${this.baseUrl}/logout`, { refreshToken } satisfies RefreshTokenRequest)
      : of(void 0);

    return request$.pipe(finalize(() => this.clearTokens()));
  }

  getRefreshToken(): string | null {
    return this.refreshTokenSignal();
  }

  clearTokens(): void {
    this.accessTokenSignal.set(null);
    this.refreshTokenSignal.set(null);
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
  }

  private setTokens(response: AuthResponse): void {
    this.accessTokenSignal.set(response.accessToken);
    this.refreshTokenSignal.set(response.refreshToken);
    localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
  }

  private decodeUser(token: string | null): DecodedUser | null {
    if (!token) {
      return null;
    }

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return { userId: payload.sub, email: payload.email };
    } catch {
      return null;
    }
  }
}
