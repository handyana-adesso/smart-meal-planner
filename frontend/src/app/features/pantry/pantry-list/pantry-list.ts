import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { PantryItem } from '../pantry-item.model';
import { MockPantryService } from '../mock-pantry.service';

@Component({
  selector: 'app-pantry-list',
  imports: [
    ReactiveFormsModule,
    DatePipe,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './pantry-list.html',
})
export class PantryList implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly pantryService = inject(MockPantryService);

  readonly items = signal<PantryItem[]>([]);
  readonly displayedColumns = ['name', 'quantity', 'expiryDate', 'actions'];

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    quantity: [1, [Validators.required, Validators.min(0.01)]],
    unit: ['', Validators.required],
    expiryDate: [''],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.pantryService.getAll().subscribe((items) => this.items.set(items));
  }

  add(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.pantryService
      .add({ ...value, expiryDate: value.expiryDate || undefined })
      .subscribe(() => {
        this.form.reset({ name: '', quantity: 1, unit: '', expiryDate: '' });
        this.load();
      });
  }

  remove(item: PantryItem): void {
    this.pantryService.remove(item.id).subscribe(() => this.load());
  }

  isExpiringSoon(item: PantryItem): boolean {
    return this.pantryService.isExpiringSoon(item);
  }
}
