import { Component, input } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { ExpenseCategoryBreakdown } from '../../core/models/expense.model';

@Component({
  selector: 'app-breakdown-list',
  imports: [CurrencyPipe, MatCardModule],
  templateUrl: './breakdown-list.html',
})
export class BreakdownList {
  readonly totalAmount = input.required<number>();
  readonly totalCount = input.required<number>();
  readonly breakdowns = input.required<ExpenseCategoryBreakdown[]>();
}
