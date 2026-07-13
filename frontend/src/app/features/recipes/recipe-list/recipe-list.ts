import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { RecipeResponse } from '../../../core/models/recipe.model';
import { RecipeService } from '../recipe.service';
import { NotificationService } from '../../../shared/notification.service';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-recipe-list',
  imports: [RouterLink, CurrencyPipe, MatTableModule, MatButtonModule, MatIconModule],
  templateUrl: './recipe-list.html',
})
export class RecipeList implements OnInit {
  private readonly recipeService = inject(RecipeService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);

  readonly recipes = signal<RecipeResponse[]>([]);
  readonly displayedColumns = ['name', 'servings', 'estimatedCost', 'actions'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.recipeService.getAll().subscribe({
      next: (recipes) => this.recipes.set(recipes),
      error: () => this.notification.error('Failed to load recipes.'),
    });
  }

  delete(recipe: RecipeResponse): void {
    const dialogRef = this.dialog.open(ConfirmDialog, {
      data: { title: 'Delete recipe', message: `Delete "${recipe.name}"? This cannot be undone.` },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.recipeService.delete(recipe.id).subscribe({
        next: () => {
          this.notification.success('Recipe deleted.');
          this.load();
        },
        error: () => this.notification.error('Failed to delete recipe.'),
      });
    });
  }
}
