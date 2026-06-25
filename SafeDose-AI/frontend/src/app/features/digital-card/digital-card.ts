import {
  AfterViewInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  DestroyRef,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { EMPTY, from, of } from 'rxjs';
import { catchError, finalize, map, switchMap } from 'rxjs/operators';
import { Auth } from '../../core/auth/services/auth';
import { CardData } from '../../core/models/card-data';
import { MedicalCardService } from '../../core/services/medical-card';
import { PatientService } from '../../core/services/patient';
import { Card } from '../../shared/components/card/card';
import {
  ArrowRight,
  Heart,
  Pill,
  Printer,
  QrCode,
  Shield,
  SquarePen,
  Trash2,
  X,
} from 'lucide-angular';

@Component({
  selector: 'app-digital-card',
  imports: [LucideAngularModule, FormsModule, Card],
  templateUrl: './digital-card.html',
  styleUrl: './digital-card.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DigitalCard implements AfterViewInit {
  private readonly auth = inject(Auth);
  private readonly medicalCardService = inject(MedicalCardService);
  protected readonly patientService = inject(PatientService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  heartIcon = Heart;
  pillIcon = Pill;
  qrCodeIcon = QrCode;
  printerIcon = Printer;
  shieldIcon = Shield;
  editIcon = SquarePen;
  trashIcon = Trash2;
  xIcon = X;
  arrowRightIcon = ArrowRight;

  loading = signal(false);
  error = signal('');
  qrImage = signal('');
  cardLoaded = signal(false);
  cardData = signal<CardData>({
    id: '',
    name: '',
    age: 0,
    medications: [],
    allergies: [],
    doctorName: '',
    qrUrl: '',
  });

  patientId = this.patientService.currentPatientId;
  private resolvePatientId() {
    return from(this.patientService.getPrimaryPatientId()).pipe(
      map((id) => (id != null ? String(id).trim() : '')),
      catchError(() => of('')),
    );
  }

  loadCard(): void {
    this.error.set('');
    this.loading.set(true);
    this.cdr.markForCheck();

    this.resolvePatientId()
      .pipe(
        switchMap((patientId) => {
          if (!patientId) {
            this.error.set('لازم تضيف بيانات المريض الأول من بوابة المريض.');
            this.cdr.markForCheck();
            return EMPTY;
          }

          return from(
            Promise.all([
              this.medicalCardService.getPrivateCard(patientId),
              this.medicalCardService.getPrivateQrCode(patientId),
            ]),
          );
        }),
        catchError(() => {
          this.error.set('تعذر تحميل البطاقة الطبية الخاصة.');
          return EMPTY;
        }),
        finalize(() => {
          this.loading.set(false);
          this.cdr.markForCheck();
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(([card, qr]) => {
        this.cardData.set(card);
        this.qrImage.set(qr);
        this.cardLoaded.set(true);
        this.cdr.markForCheck();
      });
  }

  ngAfterViewInit(): void {
    queueMicrotask(() => {
      this.loadCard();
    });
  }

  printCard(): void {
    window.print();
  }

  downloadPdf(): void {
    this.resolvePatientId()
      .pipe(
        switchMap((patientId) => {
          if (!patientId) {
            this.error.set('لم يتم العثور على معرف المريض لتحميل الملف.');
            return EMPTY;
          }
          return from(this.medicalCardService.downloadPrivatePdf(patientId));
        }),
        catchError(() => {
          this.error.set('تعذر تحميل ملف PDF للبطاقة.');
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  generateQR(): void {
    this.resolvePatientId()
      .pipe(
        switchMap((patientId) => {
          if (!patientId) {
            this.error.set('لم يتم العثور على معرف المريض لإنشاء الـ QR.');
            return EMPTY;
          }
          return from(this.medicalCardService.getPrivateQrCode(patientId));
        }),
        catchError(() => {
          this.error.set('تعذر تحميل رمز الـ QR.');
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((qrImage) => {
        this.qrImage.set(qrImage);
      });
  }
  async changePatient(patientId: number): Promise<void> {
    await this.patientService.setRunningPatient(patientId);
  }
}
