import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MonthlySpendingReportResponse } from '../../core/models/report.model';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/reports`;

  getMonthlySpending(month: number, year: number): Observable<MonthlySpendingReportResponse> {
    const params = new HttpParams().set('month', month).set('year', year);
    return this.http.get<MonthlySpendingReportResponse>(`${this.baseUrl}/monthly-spending`, { params });
  }
}
