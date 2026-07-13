import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MonthlySpendingReportResponse } from '../../../core/models/report.model';
import { ReportService } from '../report.service';
import { NotificationService } from '../../../shared/notification.service';
import { BreakdownList } from '../../../shared/breakdown-list/breakdown-list';

const MONTH_NAMES = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
];

@Component({
  selector: 'app-report-view',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatSelectModule, MatButtonModule, BreakdownList],
  templateUrl: './report-view.html',
})
export class ReportView implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly reportService = inject(ReportService);
  private readonly notification = inject(NotificationService);

  readonly report = signal<MonthlySpendingReportResponse | null>(null);
  readonly months = MONTH_NAMES.map((name, index) => ({ value: index + 1, name }));
  readonly years = Array.from({ length: 6 }, (_, i) => new Date().getFullYear() - i);

  readonly form = this.fb.nonNullable.group({
    month: new Date().getMonth() + 1,
    year: new Date().getFullYear(),
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    const { month, year } = this.form.getRawValue();
    this.reportService.getMonthlySpending(month, year).subscribe({
      next: (report) => this.report.set(report),
      error: () => this.notification.error('Failed to load monthly spending report.'),
    });
  }
}
