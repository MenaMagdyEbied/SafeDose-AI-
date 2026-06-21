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
import { ParsedMedication, SavePrescriptionPayload } from '../../core/models/prescription-api';
import { PatientService } from '../../core/services/patient';
import { Prescription } from '../../core/services/prescription';
type ViewStage = 'upload' | 'review' | 'summary';

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

  doctorName = signal<string | null>(null);
  parsedMeds = signal<ParsedMedication[]>([]);

  lastSavedPrescription = signal<{
    id: number;
    name: string;
    date: string;
    source: 'scan' | 'manual';
    meds: { name: string }[];
  } | null>(null);

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
        this.parsedMeds.set(res.medications);
        this.stage.set('review');
      });
  }

  addManually(): void {
    this.doctorName.set(null);
    this.parsedMeds.set([
      {
        drug_name_guess: '',
        dose_guess: '',
        frequency_guess: '',
        duration_guess: null,
        needsReview: true,
      },
    ]);
    this.stage.set('review');
  }

  updateMed(index: number, field: keyof ParsedMedication, value: string): void {
    this.parsedMeds.update((list) =>
      list.map((m, i) => (i === index ? { ...m, [field]: value } : m)),
    );
  }

  removeMed(index: number): void {
    this.parsedMeds.update((list) => list.filter((_, i) => i !== index));
  }

  addEmptyMed(): void {
    this.parsedMeds.update((list) => [
      ...list,
      {
        drug_name_guess: '',
        dose_guess: '',
        frequency_guess: '',
        duration_guess: null,
        needsReview: true,
      },
    ]);
  }

<<<<<<< Updated upstream
  /** ينقل من شاشة المراجعة لشاشة الحفظ الفعلي */
=======
>>>>>>> Stashed changes
  confirmAndSave(): void {
    if (!this.currentPatientId) {
      this.errorText.set('تعذر تحديد المريض. حاول مرة أخرى.');
      return;
    }

    const validMeds = this.parsedMeds().filter((m) => m.drug_name_guess.trim());
    if (validMeds.length === 0) {
      this.errorText.set('أضف دواء واحد على الأقل قبل الحفظ.');
      return;
    }

    const today = new Date().toISOString().slice(0, 10);
    const prescriptionName = 'وصفة بتاريخ ' + new Date().toLocaleDateString('ar-EG');

    const payload: SavePrescriptionPayload = {
      patientId: this.currentPatientId,
      prescriptionName,
      imageUrl: '',
      drugs: validMeds.map((m) => ({
        drugName: m.drug_name_guess.trim(),
        dose: m.dose_guess ?? '',
        doctorName: this.doctorName() ?? '',
        route: 0,
        frequency: this.parseFrequencyNumber(m.frequency_guess),
        startDate: today,
        endDate: today,
        mealTiming: 0,
      })),
    };

    this.saving.set(true);
    this.errorText.set('');

    from(this.prescriptionService.save(payload))
      .pipe(
        catchError((err) => {
          this.errorText.set(err?.error?.message || 'حدث خطأ أثناء حفظ الوصفة. حاول مرة أخرى.');
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
          meds: validMeds.map((m) => ({ name: m.drug_name_guess })),
        });
        this.stage.set('summary');
      });
  }

  viewPrescriptionDetail(): void {
    const saved = this.lastSavedPrescription();
    if (!saved) return;

    this.router.navigate(['/prescription-detail', saved.id], {
      state: {
        prescription: {
          id: saved.id,
          name: saved.name,
          date: saved.date,
          source: saved.source,
          doctorName: this.doctorName(),
          meds: this.parsedMeds()
            .filter((m) => m.drug_name_guess.trim())
            .map((m) => ({
              name: m.drug_name_guess,
              dose: m.dose_guess ?? '',
              frequency: m.frequency_guess ?? '',
              duration: m.duration_guess ?? '',
            })),
        },
      },
    });
  }

  private parseFrequencyNumber(text: string | null): number {
    if (!text) return 1;
    if (text.includes('مرتين') || text.toLowerCase().includes('twice')) return 2;
    if (text.includes('ثلاث') || text.includes('٣')) return 3;
    if (text.includes('أربع') || text.includes('٤')) return 4;
    return 1;
  }

  goHome(): void {
    this.router.navigate(['/patient']);
  }

  reset(): void {
    this.stage.set('upload');
    this.parsedMeds.set([]);
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
