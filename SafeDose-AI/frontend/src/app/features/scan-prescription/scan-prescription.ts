import { ChangeDetectorRef, Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import {
  Camera,
  CircleCheck,
  Eye,
  LucideAngularModule,
  Plus,
  RotateCcw,
  Trash2,
  TriangleAlert,
  Upload,
  X,
} from 'lucide-angular';
import { EMPTY, from } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import {
  ParsedMedication,
  SaveDrugDto,
  SavePrescriptionPayload,
} from '../../core/models/prescription-api';
import { PatientService } from '../../core/services/patient';
import { Prescription } from '../../core/services/prescription';
import { Interaction } from '../../core/services/interaction';

type ViewStage = 'upload' | 'review' | 'summary';

interface ReviewMed extends ParsedMedication {
  resolvedName: string;
  searchSuggestions: string[];
  frequencyNumber: number;
  doctorName: string;
  dose: string;
  startDate: string;
  endDate: string;
  mealTiming: number;
}
@Component({
  selector: 'app-scan-prescription',
  imports: [LucideAngularModule],
  templateUrl: './scan-prescription.html',
  styleUrl: './scan-prescription.css',
})
export class ScanPrescription implements OnInit {
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly prescriptionService = inject(Prescription);
  private readonly patientService = inject(PatientService);
  private readonly interactionService = inject(Interaction);

  stage = signal<ViewStage>('upload');
  loading = signal(false);
  saving = signal(false);
  errorText = signal('');
  successText = signal('');

  cameraIcon = Camera;
  uploadIcon = Upload;
  rotateCcwIcon = RotateCcw;
  alertTriangleIcon = TriangleAlert;
  checkCircleIcon = CircleCheck;
  videoStream = signal<MediaStream | null>(null);
  showCamera = signal(false);
  plusIcon = Plus;
  trashIcon = Trash2;
  xIcon = X;
  eyeIcon = Eye;

  private currentPatientId: number | null = null;
  private today = new Date().toISOString().slice(0, 10);

  doctorName = signal<string | null>(null);
  reviewMeds = signal<ReviewMed[]>([]);

  lastSavedPrescription = signal<{
    id: number;
    name: string;
    date: string;
    source: 'scan' | 'manual';
    meds: { name: string }[];
  } | null>(null);

  frequencyOptions = [1, 2, 3, 4];
  mealTimingOptions = [
    { value: 0, label: 'بدون تحديد' },
    { value: 1, label: 'قبل الأكل' },
    { value: 2, label: 'مع الأكل' },
    { value: 3, label: 'بعد الأكل' },
  ];

  ngOnInit(): void {
    this.patientService
      .getMyPatients()
      .then((patients) => {
        this.currentPatientId = patients[0]?.patientId ?? patients[0]?.id ?? null;
      })
      .catch(() => {
        this.currentPatientId = null;
      });
  }

  openCamera(): void {
    this.showCamera.set(true);
    this.lockBodyScroll();

    from(navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' } }))
      .pipe(
        catchError(() => {
          this.showCamera.set(false);
          this.unlockBodyScroll();
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((stream) => {
        this.videoStream.set(stream);
        setTimeout(() => {
          const video = document.getElementById('cameraFeed') as HTMLVideoElement;
          if (video) video.srcObject = this.videoStream();
        }, 100);
      });
  }

  closeCamera(): void {
    this.videoStream()
      ?.getTracks()
      .forEach((t) => t.stop());
    this.videoStream.set(null);
    this.showCamera.set(false);
    this.unlockBodyScroll();
  }

  capturePhoto(): void {
    const video = document.getElementById('cameraFeed') as HTMLVideoElement;
    const canvas = document.createElement('canvas');
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    canvas.getContext('2d')?.drawImage(video, 0, 0);
    canvas.toBlob((blob) => {
      if (blob) {
        const file = new File([blob], 'prescription.jpg', { type: 'image/jpeg' });
        this.handleFile(file);
      }
    });
    this.closeCamera();
  }

  openFilePicker(): void {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    input.onchange = (e: any) => {
      const file = e.target.files[0];
      if (file) this.handleFile(file);
    };
    input.click();
  }

  handleFile(file: File): void {
    this.loading.set(true);
    this.errorText.set('');
    this.cdr.detectChanges();

    from(this.prescriptionService.parse(file))
      .pipe(
        catchError(() => {
          this.errorText.set('حدث خطأ أثناء تحليل الوصفة. حاول مرة أخرى أو أضف الأدوية يدويًا.');
          return EMPTY;
        }),
        finalize(() => {
          this.loading.set(false);
          this.cdr.detectChanges();
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((res) => {
        this.doctorName.set(res.doctor_name);
        const meds = res.medications.map((m) => this.toReviewMed(m));
        this.reviewMeds.set(meds);
        this.stage.set('review');

        // بحث تلقائي في الكتالوج لكل دواء بناءً على الاسم المخمّن
        meds.forEach((m, i) => this.searchForMed(i, m.resolvedName));
      });
  }

  private toReviewMed(m: ParsedMedication): ReviewMed {
    return {
      ...m,
      resolvedName: m.drug_name_guess ?? '',
      searchSuggestions: [],
      frequencyNumber: this.parseFrequencyNumber(m.frequency_guess),
      doctorName: this.doctorName() ?? '',
      dose: m.dose_guess ?? '',
      startDate: this.today,
      endDate: this.today,
      mealTiming: 0,
    };
  }

  /** بيدور في /drugs/search عشان يطلع اقتراحات حقيقية من الكتالوج لكل دواء */
  private searchForMed(index: number, query: string): void {
    if (!query?.trim()) return;

    from(this.interactionService.searchDrugs(query))
      .pipe(
        catchError(() => EMPTY),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((results) => {
        const names = results.map((r) => r.commercialNameAr || r.commercialNameEn);
        this.reviewMeds.update((list) =>
          list.map((m, i) => (i === index ? { ...m, searchSuggestions: names } : m)),
        );
      });
  }

  onDrugNameInput(index: number, value: string): void {
    this.reviewMeds.update((list) =>
      list.map((m, i) => (i === index ? { ...m, resolvedName: value } : m)),
    );
    this.searchForMed(index, value);
  }

  selectSuggestion(index: number, name: string): void {
    this.reviewMeds.update((list) =>
      list.map((m, i) => (i === index ? { ...m, resolvedName: name, searchSuggestions: [] } : m)),
    );
  }

  updateMedField(index: number, field: keyof ReviewMed, value: any): void {
    this.reviewMeds.update((list) =>
      list.map((m, i) => (i === index ? { ...m, [field]: value } : m)),
    );
  }

  removeMed(index: number): void {
    this.reviewMeds.update((list) => list.filter((_, i) => i !== index));
  }

  addEmptyMed(): void {
    this.reviewMeds.update((list) => [
      ...list,
      {
        drug_name_guess: '',
        dose_guess: '',
        frequency_guess: '',
        duration_guess: null,
        needsReview: true,
        resolvedName: '',
        searchSuggestions: [],
        frequencyNumber: 1,
        doctorName: this.doctorName() ?? '',
        dose: '',
        startDate: this.today,
        endDate: this.today,
        mealTiming: 0,
      },
    ]);
  }

  addManually(): void {
    this.doctorName.set(null);
    this.reviewMeds.set([]);
    this.addEmptyMed();
    this.stage.set('review');
  }

  confirmAndSave(): void {
    if (!this.currentPatientId) {
      this.errorText.set('تعذر تحديد المريض. حاول مرة أخرى.');
      return;
    }

    const validMeds = this.reviewMeds().filter((m) => m.resolvedName.trim());
    if (validMeds.length === 0) {
      this.errorText.set('أضف دواء واحد على الأقل قبل الحفظ.');
      return;
    }

    const prescriptionName = 'وصفة بتاريخ ' + new Date().toLocaleDateString('ar-EG');

    const drugs: SaveDrugDto[] = validMeds.map((m) => ({
      drugName: m.resolvedName.trim(),
      dose: m.dose,
      doctorName: m.doctorName,
      route: 0,
      frequency: m.frequencyNumber,
      startDate: m.startDate,
      endDate: m.endDate,
      mealTiming: m.mealTiming,
    }));

    const payload: SavePrescriptionPayload = {
      patientId: this.currentPatientId,
      prescriptionName,
      imageUrl: '',
      drugs,
    };

    this.saving.set(true);
    this.errorText.set('');

    from(this.prescriptionService.save(payload))
      .pipe(
        catchError((err) => {
          this.errorText.set(
            (typeof err?.error === 'string' ? err.error : err?.error?.message) ||
              'حدث خطأ أثناء حفظ الوصفة. حاول مرة أخرى.',
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
        this.lastSavedPrescription.set({
          id: result.prescriptionId,
          name: prescriptionName,
          date: new Date().toLocaleDateString('ar-EG'),
          source: this.doctorName() ? 'scan' : 'manual',
          meds: validMeds.map((m) => ({ name: m.resolvedName })),
        });
        this.stage.set('summary');
      });
  }

  viewPrescriptionDetail(): void {
    const saved = this.lastSavedPrescription();
    if (!saved) return;
    this.router.navigate(['/prescription-detail', saved.id]);
  }

  private parseFrequencyNumber(text: string | null): number {
    if (!text) return 1;
    const t = text.toLowerCase();

    if (t.includes('مرتين') || t.includes('twice') || t.includes('2 times') || t.includes('2x'))
      return 2;
    if (t.includes('ثلاث') || t.includes('٣') || t.includes('3 times') || t.includes('3x'))
      return 3;
    if (t.includes('أربع') || t.includes('٤') || t.includes('4 times') || t.includes('4x'))
      return 4;

    const everyHoursMatch = t.match(/every\s+(\d+)\s+hours?/);
    if (everyHoursMatch) {
      const hours = parseInt(everyHoursMatch[1], 10);
      if (hours > 0) return Math.max(1, Math.round(24 / hours));
    }

    return 1;
  }
  goHome(): void {
    this.router.navigate(['/patient-home']);
  }

  reset(): void {
    this.stage.set('upload');
    this.reviewMeds.set([]);
    this.doctorName.set(null);
    this.lastSavedPrescription.set(null);
    this.errorText.set('');
    this.successText.set('');
  }

  private originalBodyOverflow: string | null = null;

  private lockBodyScroll(): void {
    if (this.originalBodyOverflow === null) {
      this.originalBodyOverflow = document.body.style.overflow;
    }
    document.body.style.overflow = 'hidden';
  }

  private unlockBodyScroll(): void {
    document.body.style.overflow = this.originalBodyOverflow ?? '';
    this.originalBodyOverflow = null;
  }
}
