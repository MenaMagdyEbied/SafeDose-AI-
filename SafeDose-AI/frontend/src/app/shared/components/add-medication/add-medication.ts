import {
  ChangeDetectorRef,
  Component,
  DestroyRef,
  EventEmitter,
  OnInit,
  Output,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormArray,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  AlarmClock,
  CalendarDays,
  ChevronDown,
  CircleCheck,
  LucideAngularModule,
  Pill,
  Plus,
  Search,
  Trash2,
  TriangleAlert,
  X,
} from 'lucide-angular';
import { EMPTY, from } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { Medications } from '../../../core/services/medications';
import { Interaction } from '../../../core/services/interaction';
import { PatientService } from '../../../core/services/patient';
import {
  AddMedicationPayload,
  DrugSearchResult,
  MAX_FREQUENCY,
  MEAL_TIMING_OPTIONS,
  MedicationResponse,
} from '../../../core/models';
import { timesMatchFrequency } from '../../validators/times-match-frequency';
import { dateRangeValid } from '../../validators/date-range-valid';

@Component({
  selector: 'app-add-medication',
  imports: [ReactiveFormsModule, LucideAngularModule],
  templateUrl: './add-medication.html',
  styleUrl: './add-medication.css',
})
export class AddMedication implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly medicationsService = inject(Medications);
  private readonly interaction = inject(Interaction);
  private readonly patientService = inject(PatientService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  @Output() saved = new EventEmitter<MedicationResponse>();
  @Output() cancelled = new EventEmitter<void>();
  // Icons
  pillIcon = Pill;
  searchIcon = Search;
  xIcon = X;
  plusIcon = Plus;
  trashIcon = Trash2;
  alarmClockIcon = AlarmClock;
  calendarIcon = CalendarDays;
  chevronDownIcon = ChevronDown;
  checkCircleIcon = CircleCheck;
  alertTriangleIcon = TriangleAlert;

  mealTimingOptions = MEAL_TIMING_OPTIONS;
  frequencyOptions = Array.from({ length: MAX_FREQUENCY }, (_, i) => i + 1);

  saving = signal(false);
  errorText = signal('');
  successText = signal('');

  // Drug search/autocomplete state
  resultsOpen = signal(false);
  selectedCatalogDrug = signal<DrugSearchResult | null>(null);
  readonly filteredDrugs = computed(() => this.interaction.searchResults());

  // Editing an existing medication (route param :id) vs adding a new one
  editingId = signal<number | null>(null);
  isEditMode = computed(() => this.editingId() !== null);

  private currentPatientId: number | null = null;

  form = this.fb.group(
    {
      drugName: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(255)]],
      dose: [''],
      doctorName: [''],
      frequency: [null as number | null, [Validators.min(1), Validators.max(MAX_FREQUENCY)]],
      times: this.fb.array<AbstractControl>([]),
      mealTiming: [null as number | null],
      startDate: [this.todayIso()],
      endDate: [''],
    },
    { validators: [timesMatchFrequency(), dateRangeValid()] },
  );

  get timesArray(): FormArray {
    return this.form.get('times') as FormArray;
  }

  ngOnInit(): void {
    this.syncPatientContext();

    effect(() => {
      const patientId = this.patientService.currentPatientId;
      if (patientId != null && patientId !== this.currentPatientId) {
        this.currentPatientId = patientId;
      }
    });

    // Watch frequency changes -> keep the times FormArray in sync with the chosen count
    this.form
      .get('frequency')
      ?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => this.syncTimesArrayLength(Number(value) || 0));

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = Number(idParam);
      this.editingId.set(id);
      this.loadForEdit(id);
    }
  }

  private async syncPatientContext(): Promise<void> {
    try {
      this.currentPatientId =
        this.patientService.currentPatientId ?? (await this.patientService.getPrimaryPatientId());
    } catch {
      this.currentPatientId = null;
    }
  }

  private todayIso(): string {
    return new Date().toISOString().slice(0, 10);
  }

  private syncTimesArrayLength(count: number): void {
    const arr = this.timesArray;
    while (arr.length < count) {
      arr.push(this.fb.control('', Validators.required));
    }
    while (arr.length > count) {
      arr.removeAt(arr.length - 1);
    }
    arr.updateValueAndValidity();
    this.form.updateValueAndValidity();
  }

  // ===== Drug autocomplete (same endpoint/pattern as interaction-checker) =====

  onDrugNameInput(value: string): void {
    this.selectedCatalogDrug.set(null);
    this.resultsOpen.set(true);

    if (!value.trim()) {
      this.interaction.searchResults.set([]);
      return;
    }

    from(this.interaction.searchDrugs(value))
      .pipe(
        catchError(() => EMPTY),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  selectDrug(drug: DrugSearchResult): void {
    this.selectedCatalogDrug.set(drug);
    this.form.patchValue({ drugName: drug.commercialNameAr || drug.commercialNameEn });
    this.resultsOpen.set(false);
  }

  closeResults(): void {
    setTimeout(() => this.resultsOpen.set(false), 150);
  }

  save(): void {
    this.errorText.set('');
    this.successText.set('');

    if (this.form.invalid || !this.currentPatientId) {
      this.form.markAllAsTouched();
      this.timesArray.controls.forEach((c) => c.markAsTouched());
      if (!this.currentPatientId) {
        this.errorText.set('تعذر تحديد المريض. حاول مرة أخرى.');
      }
      return;
    }

    const raw = this.form.getRawValue();

    const payload: AddMedicationPayload = {
      patientId: this.currentPatientId,
      drugName: raw.drugName!.trim(),
      drugCatalogId: this.selectedCatalogDrug()?.drugCatalogId ?? null,
      dose: raw.dose || null,
      doctorName: raw.doctorName || null,
      frequency: raw.frequency ?? null,
      startDate: raw.startDate || null,
      endDate: raw.endDate || null,
      mealTiming: (raw.mealTiming as any) ?? null,
      times: raw.frequency
        ? (raw.times as string[]).map((t) => (t.length === 5 ? `${t}:00` : t))
        : null,
    };

    this.saving.set(true);
    this.cdr.detectChanges();

    const id = this.editingId();
    const request$ = id
      ? from(
          this.medicationsService.update(id, {
            drugName: payload.drugName,
            dose: payload.dose,
            frequency: payload.frequency,
            startDate: payload.startDate,
            endDate: payload.endDate,
            mealTiming: payload.mealTiming,
            times: payload.times,
          }),
        )
      : from(this.medicationsService.addManually(payload));

    request$
      .pipe(
        catchError((err) => {
          this.errorText.set(
            err?.error?.message || 'حدث خطأ أثناء حفظ الدواء. تأكد من البيانات وحاول مرة أخرى.',
          );
          return EMPTY;
        }),
        finalize(() => {
          this.saving.set(false);
          this.cdr.detectChanges();
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((result) => {
        this.successText.set('تم حفظ الدواء بنجاح');
        // ✅ بدل router.navigate، نطلق event للـ parent بعد نص ثانية (يدي وقت يشوف رسالة النجاح)
        setTimeout(() => this.saved.emit(result), 600);
      });
  }

  private loadForEdit(id: number): void {
    from(this.medicationsService.getById(id))
      .pipe(
        catchError(() => {
          this.errorText.set('تعذر تحميل بيانات الدواء.');
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((med) => {
        if (med.frequency) this.syncTimesArrayLength(med.frequency);
        this.form.patchValue({
          drugName: med.drugName,
          dose: med.dose ?? '',
          frequency: med.frequency ?? null,
          startDate: med.startDate ?? this.todayIso(),
          endDate: med.endDate ?? '',
          mealTiming: med.mealTiming ?? null,
        });
        med.times.forEach((t, i) => {
          if (this.timesArray.at(i)) this.timesArray.at(i).setValue(t.slice(0, 5));
        });
        if (med.drugCatalogId) {
          this.selectedCatalogDrug.set({
            drugCatalogId: med.drugCatalogId,
            commercialNameAr: med.drugName,
            commercialNameEn: med.drugName,
          } as DrugSearchResult);
        }
        this.cdr.detectChanges();
      });
  }

  cancel(): void {
    this.cancelled.emit();
  }
}
