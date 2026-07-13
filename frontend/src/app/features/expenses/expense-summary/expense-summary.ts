import { Component, OnInit, inject, signal } from '@angular/core';
import { ExpenseSummaryResponse } from '../../../core/models/expense.model';
import { ExpenseService } from '../expense.service';
import { NotificationService } from '../../../shared/notification.service';
import { BreakdownList } from '../../../shared/breakdown-list/breakdown-list';

@Component({
  selector: 'app-expense-summary',
  imports: [BreakdownList],
  templateUrl: './expense-summary.html',
})
export class ExpenseSummary implements OnInit {
  private readonly expenseService = inject(ExpenseService);
  private readonly notification = inject(NotificationService);

  readonly summary = signal<ExpenseSummaryResponse | null>(null);

  ngOnInit(): void {
    this.expenseService.getSummary().subscribe({
      next: (summary) => this.summary.set(summary),
      error: () => this.notification.error('Failed to load expense summary.'),
    });
  }
}
