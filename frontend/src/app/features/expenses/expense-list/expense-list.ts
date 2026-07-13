import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialog } from '@angular/material/dialog';
import { EXPENSE_CATEGORY_LABELS, ExpenseCategory, GroceryExpenseResponse } from '../../../core/models/expense.model';
import { ExpenseService } from '../expense.service';
import { NotificationService } from '../../../shared/notification.service';
import { ConfirmDialog } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-expense-list',
  imports: [
    RouterLink,
    CurrencyPipe,
    DatePipe,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatFormFieldModule,
  ],
  templateUrl: './expense-list.html',
})
export class ExpenseList implements OnInit {
  private readonly expenseService = inject(ExpenseService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);

  readonly expenses = signal<GroceryExpenseResponse[]>([]);
  readonly displayedColumns = ['description', 'category', 'amount', 'date', 'actions'];
  readonly categoryLabels = EXPENSE_CATEGORY_LABELS;
  readonly categories = Object.values(ExpenseCategory).filter(
    (value): value is ExpenseCategory => typeof value === 'number',
  );

  readonly selectedCategory = signal<ExpenseCategory | 'all'>('all');

  ngOnInit(): void {
    this.load();
  }

  onCategoryChange(category: ExpenseCategory | 'all'): void {
    this.selectedCategory.set(category);
    this.load();
  }

  load(): void {
    const category = this.selectedCategory();
    const request$ = category === 'all' ? this.expenseService.getAll() : this.expenseService.getByCategory(category);

    request$.subscribe({
      next: (expenses) => this.expenses.set(expenses),
      error: () => this.notification.error('Failed to load expenses.'),
    });
  }

  delete(expense: GroceryExpenseResponse): void {
    const dialogRef = this.dialog.open(ConfirmDialog, {
      data: { title: 'Delete expense', message: `Delete "${expense.description}"? This cannot be undone.` },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.expenseService.delete(expense.id).subscribe({
        next: () => {
          this.notification.success('Expense deleted.');
          this.load();
        },
        error: () => this.notification.error('Failed to delete expense.'),
      });
    });
  }
}
