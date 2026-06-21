import { AbstractControl, FormArray, ValidationErrors, ValidatorFn } from "@angular/forms";

// Cross-field validator: number of time controls must equal the chosen frequency.
export function timesMatchFrequency(): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const frequency = group.get('frequency')?.value;
    const times = group.get('times') as FormArray | null;
    if (!frequency || !times) return null;
    if (times.length !== Number(frequency)) {
      return { timesFrequencyMismatch: true };
    }
    const hasEmptyTime = times.controls.some((c) => !c.value);
    if (hasEmptyTime) return { timesIncomplete: true };
    return null;
  };
}