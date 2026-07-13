import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { EXPENSE_CATEGORY_LABELS, ExpenseCategory, GroceryExpenseRequest } from '../../../core/models/expense.model';
import { RecipeResponse } from '../../../core/models/recipe.model';
import { ExpenseService } from '../expense.service';
import { RecipeService } from '../../recipes/recipe.service';
import { NotificationService } from '../../../shared/notification.service';

@Component({
  selector: 'app-expense-form',
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
  ],
  templateUrl: './expense-form.html',
})
export class ExpenseForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly expenseService = inject(ExpenseService);
  private readonly recipeService = inject(RecipeService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly notification = inject(NotificationService);

  readonly loading = signal(false);
  readonly expenseId = signal<string | null>(null);
  readonly isEditMode = computed(() => this.expenseId() !== null);
  readonly recipes = signal<RecipeResponse[]>([]);
  readonly categoryLabels = EXPENSE_CATEGORY_LABELS;
  readonly categories = Object.values(ExpenseCategory).filter(
    (value): value is ExpenseCategory => typeof value === 'number',
  );

  readonly form = this.fb.nonNullable.group({
    description: ['', [Validators.required, Validators.maxLength(200)]],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    category: [ExpenseCategory.Groceries, Validators.required],
    date: [new Date()],
    recipeId: [''],
  });

  ngOnInit(): void {
    this.recipeService.getAll().subscribe({ next: (recipes) => this.recipes.set(recipes) });

    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.expenseId.set(id);
    this.expenseService.getById(id).subscribe({
      next: (expense) =>
        this.form.patchValue({
          description: expense.description,
          amount: expense.amount,
          category: expense.category,
          date: new Date(expense.date),
          recipeId: expense.recipeId ?? '',
        }),
      error: () => this.notification.error('Failed to load expense.'),
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: GroceryExpenseRequest = {
      description: value.description,
      amount: value.amount,
      category: value.category,
      date: value.date?.toISOString(),
      recipeId: value.recipeId || undefined,
    };

    this.loading.set(true);
    const id = this.expenseId();
    const request$ = id ? this.expenseService.update(id, request) : this.expenseService.create(request);

    request$.subscribe({
      next: () => {
        this.loading.set(false);
        this.notification.success(id ? 'Expense updated.' : 'Expense created.');
        this.router.navigate(['/expenses']);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.notification.error(
          err.status === 404 ? 'Selected recipe was not found.' : 'Failed to save expense.',
        );
      },
    });
  }
}
