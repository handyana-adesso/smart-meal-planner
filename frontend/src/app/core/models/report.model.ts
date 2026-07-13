import { ExpenseCategoryBreakdown } from './expense.model';

export interface MonthlySpendingReportResponse {
  month: number;
  year: number;
  totalAmount: number;
  totalCount: number;
  breakdowns: ExpenseCategoryBreakdown[];
}
