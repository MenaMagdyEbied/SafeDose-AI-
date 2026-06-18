import {
  AfterViewInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  DestroyRef,
  inject,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { EMPTY, from, of } from 'rxjs';
import { catchError, finalize, map, switchMap } from 'rxjs/operators';
import { Auth } from '../../core/auth/services/auth';
import { UserProfile } from '../../core/auth/services/user-profile';
import { CardData } from '../../core/models/card-data';
import { MedicalCardService } from '../../core/services/medical-card';
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
  private readonly userProfileService = inject(UserProfile);
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

  loading = false;
  error = '';
  qrImage = '';

  private _cardData: CardData = {
    id: '',
    name: '',
    age: 0,
    medications: [],
    allergies: [],
    doctorName: '',
    qrUrl: '',
  };

  get cardData(): CardData {
    return this._cardData;
  }
  cardLoaded = false;

  private resolvePatientId() {
    const fallback = 'd5b564aa-4a5b-4752-ba90-a0c822e71d9e';
    return from(this.userProfileService.getUserProfile()).pipe(
      map((profile: any) => {
        const id = profile.patientId ?? profile.id ?? profile.userId;
        return id ? String(id).trim() : fallback;
      }),
      catchError(() => of(fallback)),
    );
  }

  loadCard(): void {
    this.error = '';
    this.loading = true;
    this.cdr.markForCheck();

    this.resolvePatientId()
      .pipe(
        switchMap((patientId) => {
          if (!patientId) {
            this.error = 'لم يتم العثور على معرف المريض.';
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
          this.error = 'تعذر تحميل البطاقة الطبية الخاصة.';
          return EMPTY;
        }),
        finalize(() => {
          this.loading = false;
          this.cdr.markForCheck();
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(([card, qr]) => {
        this._cardData = card;
        this.qrImage = qr;
        this.cardLoaded = true;
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
            this.error = 'لم يتم العثور على معرف المريض لتحميل الملف.';
            return EMPTY;
          }

          return from(this.medicalCardService.downloadPrivatePdf(patientId));
        }),
        catchError(() => {
          this.error = 'تعذر تحميل ملف PDF للبطاقة.';
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
            this.error = 'لم يتم العثور على معرف المريض لإنشاء الـ QR.';
            return EMPTY;
          }

          return from(this.medicalCardService.getPrivateQrCode(patientId));
        }),
        catchError(() => {
          this.error = 'تعذر تحميل رمز الـ QR.';
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((qrImage) => {
        this.qrImage = qrImage;
      });
  }
}
