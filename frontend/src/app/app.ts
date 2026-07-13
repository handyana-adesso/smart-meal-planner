import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from './core/auth/auth.service';

interface NavLink {
  path: string;
  label: string;
}

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatMenuModule,
    MatDividerModule,
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isAuthenticated = this.authService.isAuthenticated;
  readonly currentUser = this.authService.currentUser;
  readonly initials = computed(() => this.currentUser()?.email.slice(0, 2).toUpperCase() ?? '');

  readonly navLinks: NavLink[] = [
    { path: '/recipes', label: 'Recipes' },
    { path: '/expenses', label: 'Expenses' },
    { path: '/reports', label: 'Reports' },
    { path: '/meal-planning', label: 'Meal Planning' },
    { path: '/grocery-list', label: 'Grocery List' },
    { path: '/pantry', label: 'Pantry' },
  ];

  logout(): void {
    this.authService.logout().subscribe(() => this.router.navigate(['/login']));
  }
}
