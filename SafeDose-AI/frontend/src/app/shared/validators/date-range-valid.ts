import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function dateRangeValid(): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const start = group.get('startDate')?.value;
    const end = group.get('endDate')?.value;
    if (!start || !end) return null;
    return new Date(end) < new Date(start) ? { dateRangeInvalid: true } : null;
  }; 
}
