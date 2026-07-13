import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MealPlanEntry, MealType } from '../meal-plan.model';
import { MockMealPlanService } from '../mock-meal-plan.service';

@Component({
  selector: 'app-meal-plan-list',
  imports: [
    ReactiveFormsModule,
    DatePipe,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  templateUrl: './meal-plan-list.html',
})
export class MealPlanList implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly mealPlanService = inject(MockMealPlanService);

  readonly entries = signal<MealPlanEntry[]>([]);
  readonly displayedColumns = ['date', 'mealType', 'recipeName', 'actions'];
  readonly mealTypes: MealType[] = ['Breakfast', 'Lunch', 'Dinner'];

  readonly form = this.fb.nonNullable.group({
    date: [new Date().toISOString().substring(0, 10), Validators.required],
    mealType: ['Dinner' as MealType, Validators.required],
    recipeName: ['', Validators.required],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.mealPlanService.getAll().subscribe((entries) => this.entries.set(entries));
  }

  add(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.mealPlanService.add(this.form.getRawValue()).subscribe(() => {
      this.form.reset({ date: new Date().toISOString().substring(0, 10), mealType: 'Dinner', recipeName: '' });
      this.load();
    });
  }

  remove(entry: MealPlanEntry): void {
    this.mealPlanService.remove(entry.id).subscribe(() => this.load());
  }
}
