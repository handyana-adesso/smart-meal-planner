export type MealType = 'Breakfast' | 'Lunch' | 'Dinner';

export interface MealPlanEntry {
  id: string;
  date: string;
  mealType: MealType;
  recipeName: string;
  notes?: string;
}
