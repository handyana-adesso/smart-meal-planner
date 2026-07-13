export interface IngredientRequest {
  name: string;
  quantity: number;
  unit: string;
  pricePerUnit: number;
}

export interface IngredientResponse {
  id: string;
  name: string;
  quantity: number;
  unit: string;
  pricePerUnit: number;
  totalCost: number;
}

export interface RecipeRequest {
  name: string;
  description?: string;
  servings: number;
  ingredients?: IngredientRequest[];
}

export interface RecipeResponse {
  id: string;
  name: string;
  description: string;
  servings: number;
  estimatedCost: number;
  createdAt: string;
  ingredients: IngredientResponse[];
}
