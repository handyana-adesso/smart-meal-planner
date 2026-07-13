import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { RecipeRequest, RecipeResponse } from '../../../core/models/recipe.model';
import { RecipeService } from '../recipe.service';
import { NotificationService } from '../../../shared/notification.service';

function buildIngredientGroup(
  fb: NonNullableFormBuilder,
  name = '',
  quantity = 0,
  unit = '',
  pricePerUnit = 0,
) {
  return fb.group({
    name: [name, Validators.required],
    quantity: [quantity, [Validators.required, Validators.min(0.01)]],
    unit: [unit, Validators.required],
    pricePerUnit: [pricePerUnit, [Validators.required, Validators.min(0)]],
  });
}

@Component({
  selector: 'app-recipe-form',
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './recipe-form.html',
})
export class RecipeForm implements OnInit {
  private readonly fb = inject(FormBuilder).nonNullable;
  private readonly recipeService = inject(RecipeService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notification = inject(NotificationService);

  readonly loading = signal(false);
  readonly recipeId = signal<string | null>(null);
  readonly isEditMode = computed(() => this.recipeId() !== null);

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(1000)]],
    servings: [1, [Validators.required, Validators.min(1)]],
    ingredients: this.fb.array<ReturnType<typeof buildIngredientGroup>>([]),
  });

  get ingredients() {
    return this.form.controls.ingredients;
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.recipeId.set(id);
    this.recipeService.getById(id).subscribe({
      next: (recipe) => this.patchForm(recipe),
      error: () => this.notification.error('Failed to load recipe.'),
    });
  }

  addIngredientRow(): void {
    this.ingredients.push(buildIngredientGroup(this.fb));
  }

  removeIngredientRow(index: number): void {
    this.ingredients.removeAt(index);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: RecipeRequest = {
      name: value.name,
      description: value.description,
      servings: value.servings,
      ingredients: value.ingredients,
    };

    this.loading.set(true);
    const id = this.recipeId();
    const request$ = id ? this.recipeService.update(id, request) : this.recipeService.create(request);

    request$.subscribe({
      next: () => {
        this.loading.set(false);
        this.notification.success(id ? 'Recipe updated.' : 'Recipe created.');
        this.router.navigate(['/recipes']);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.notification.error(
          err.status === 409 ? 'A recipe with that name already exists.' : 'Failed to save recipe.',
        );
      },
    });
  }

  private patchForm(recipe: RecipeResponse): void {
    this.form.patchValue({
      name: recipe.name,
      description: recipe.description,
      servings: recipe.servings,
    });

    recipe.ingredients.forEach((ingredient) => {
      this.ingredients.push(
        buildIngredientGroup(this.fb, ingredient.name, ingredient.quantity, ingredient.unit, ingredient.pricePerUnit),
      );
    });
  }
}
