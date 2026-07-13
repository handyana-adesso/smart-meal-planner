export enum ExpenseCategory {
  Groceries = 0,
  Household = 1,
  EatingOut = 2,
}

export const EXPENSE_CATEGORY_LABELS: Record<ExpenseCategory, string> = {
  [ExpenseCategory.Groceries]: 'Groceries',
  [ExpenseCategory.Household]: 'Household',
  [ExpenseCategory.EatingOut]: 'Eating Out',
};

export interface GroceryExpenseRequest {
  description: string;
  amount: number;
  category: ExpenseCategory;
  date?: string;
  recipeId?: string;
}

export interface GroceryExpenseResponse {
  id: string;
  description: string;
  amount: number;
  category: ExpenseCategory;
  categoryName: string;
  date: string;
  createdAt: string;
  recipeId?: string;
  recipeName?: string;
}

export interface ExpenseCategoryBreakdown {
  category: ExpenseCategory;
  categoryName: string;
  totalAmount: number;
  count: number;
  percentage: number;
}

export interface ExpenseSummaryResponse {
  totalAmount: number;
  totalCount: number;
  breakdowns: ExpenseCategoryBreakdown[];
}
