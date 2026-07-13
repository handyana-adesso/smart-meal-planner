import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatListModule } from '@angular/material/list';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { GroceryListItem } from '../grocery-item.model';
import { MockGroceryListService } from '../mock-grocery-list.service';

@Component({
  selector: 'app-grocery-list-view',
  imports: [
    ReactiveFormsModule,
    MatListModule,
    MatCheckboxModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './grocery-list-view.html',
})
export class GroceryListView implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly groceryListService = inject(MockGroceryListService);

  readonly items = signal<GroceryListItem[]>([]);

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    quantity: [1, [Validators.required, Validators.min(0.01)]],
    unit: ['', Validators.required],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.groceryListService.getAll().subscribe((items) => this.items.set(items));
  }

  add(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.groceryListService.add(this.form.getRawValue()).subscribe(() => {
      this.form.reset({ name: '', quantity: 1, unit: '' });
      this.load();
    });
  }

  toggle(item: GroceryListItem): void {
    this.groceryListService.toggleChecked(item.id).subscribe(() => this.load());
  }

  remove(item: GroceryListItem): void {
    this.groceryListService.remove(item.id).subscribe(() => this.load());
  }
}
