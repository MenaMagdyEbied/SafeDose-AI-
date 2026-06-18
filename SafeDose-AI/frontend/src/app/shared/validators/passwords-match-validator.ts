import { AbstractControl, ValidationErrors } from '@angular/forms';

export function passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
  const password = group.get('password')?.value;
  const confirmPassword = group.get('confirmPassword')?.value;
  if (password && confirmPassword && password !== confirmPassword) {
    return { passwordsMismatch: true };
  }
  return null;
}
