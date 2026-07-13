import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const passwordComplexityValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const value = control.value as string | null;
  if (!value) {
    return null;
  }

  const errors: ValidationErrors = {};
  if (!/[A-Z]/.test(value)) {
    errors['missingUppercase'] = true;
  }
  if (!/[a-z]/.test(value)) {
    errors['missingLowercase'] = true;
  }
  if (!/[0-9]/.test(value)) {
    errors['missingNumber'] = true;
  }

  return Object.keys(errors).length > 0 ? errors : null;
};
