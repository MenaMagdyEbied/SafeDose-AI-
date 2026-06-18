import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  ArrowLeft,
  CalendarRange,
  HeartPulse,
  Info,
  LucideAngularModule,
  ShieldAlert,
  User,
} from 'lucide-angular';
import { EMPTY, from } from 'rxjs';
import { catchError, distinctUntilChanged, filter, finalize, map, switchMap } from 'rxjs/operators';
import { Patient } from '../../core/models/patient';
import { PatientService } from '../../core/services/patient';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-patient-details',
  imports: [RouterLink, LucideAngularModule],
  templateUrl: './patient-details.html',
  styleUrl: './patient-details.css',
})
export class PatientDetails implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly patientService = inject(PatientService);
  private readonly destroyRef = inject(DestroyRef);

  readonly backIcon = ArrowLeft;
  readonly userIcon = User;
  readonly calendarIcon = CalendarRange;
  readonly heartIcon = HeartPulse;
  readonly shieldIcon = ShieldAlert;
  readonly infoIcon = Info;

  patient = signal<Patient | null>(null);
  loading = signal(false);
  error = signal('');

  private currentPatientId: number | null = null;

  ngOnInit(): void {
    this.route.paramMap
      .pipe(
        map((params) => params.get('id')),
        distinctUntilChanged(),
        filter((rawId): rawId is string => Boolean(rawId) && rawId !== 'undefined'),
        map((rawId) => Number(rawId)),
        filter((id): id is number => Number.isFinite(id) && id > 0),
        switchMap((id) => {
          this.currentPatientId = id;
          this.loading.set(true);
          this.error.set('');
          this.patient.set(null);

          return from(this.patientService.getPatientById(id)).pipe(
            map((loadedPatient) => ({ id, loadedPatient })),
            catchError(() => {
              if (this.currentPatientId === id) {
                this.error.set('تعذر تحميل بيانات المريض.');
              }
              return EMPTY;
            }),
            finalize(() => {
              if (this.currentPatientId === id) {
                this.loading.set(false);
              }
            }),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(({ id, loadedPatient }) => {
        if (this.currentPatientId === id) {
          this.patient.set(loadedPatient);
        }
      });
  }

  get genderLabel(): string {
    return this.patient()?.gender === 1 ? 'أنثى' : 'ذكر';
  }
}
