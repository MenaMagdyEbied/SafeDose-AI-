import { ChangeDetectorRef, Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
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
import { ScannedMed } from '../../core/models/scanned-med';
import { EMPTY, from } from 'rxjs';
import { catchError, finalize, map, switchMap } from 'rxjs/operators';
import { AddMedication } from '../../shared/components/add-medication/add-medication';

@Component({
  selector: 'app-caregiver-review',
  imports: [LucideAngularModule, RouterLink, AddMedication],
  templateUrl: './caregiver-review.html',
  styleUrl: './caregiver-review.css',
})
export class CaregiverReview implements OnInit {
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  scanned = signal(false);
  loading = signal(false);
  errorText = signal('');
  scannedMeds: ScannedMed[] = [];
  prescriptions = signal<any[]>([]);

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

  // Delete
  showDeleteConfirm = signal(false);
  prescriptionToDelete = signal<any>(null);

  // Manual Modal
  showManualModal = signal(false);
  manualForm: {
    name: string;
    meds: { name: string; dose: string; frequency: string; duration: string }[];
  } = {
    name: '',
    meds: [{ name: '', dose: '', frequency: '', duration: '' }],
  };

  commonMeds = [
    'بنادول اكسترا',
    'بنادول كولد',
    'بروفين',
    'أسبرين',
    'أموكسيسيلين',
    'أزيثروميسين',
    'ميتفورمين',
    'إنسولين',
    'أتورفاستاتين',
    'أملوديبين',
    'ليزينوبريل',
    'أوميبرازول',
    'فلوكستين',
    'باراسيتامول',
    'ديكلوفيناك',
    'كلاريتين',
    'سيتريزين',
    'فيتامين د',
    'كالسيوم',
  ];

  currentSuggestionsMap = new Map<object, string[]>();

  onMedInput(med: any, event: Event) {
    const val = (event.target as HTMLInputElement).value.trim();
    if (val.length < 1) {
      this.currentSuggestionsMap.delete(med);
      return;
    }
    const filtered = this.commonMeds.filter((m) => m.includes(val));
    this.currentSuggestionsMap.set(med, filtered);
  }

  activeSuggestions(med: any): string[] {
    return this.currentSuggestionsMap.get(med) ?? [];
  }

  selectSuggestion(med: any, name: string) {
    med.name = name;
    this.currentSuggestionsMap.delete(med);
  }

  clearSuggestions() {
    setTimeout(() => this.currentSuggestionsMap.clear(), 150);
  }

  ngOnInit() {
    if (this.prescriptions().length > 0) {
      this.scanned.set(true);
    }
  }

  openCamera(): void {
    this.showCamera.set(true);
    this.lockBodyScroll();

    from(
      navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' },
      }),
    )
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
    const stream = this.videoStream();
    stream?.getTracks().forEach((t) => t.stop());
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

    from(this.fileToBase64(file))
      .pipe(
        switchMap((base64) => this.extractMedsFromImage(base64, file.type)),
        catchError(() => {
          this.errorText.set('حدث خطأ أثناء تحليل الوصفة. حاول مرة أخرى.');
          return from([this.getMockMeds()]);
        }),
        finalize(() => {
          this.loading.set(false);
          this.cdr.detectChanges();
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((meds) => {
        this.openReviewModal(meds);
      });
  }
  openReviewModal(meds: ScannedMed[]): void {
    this.manualForm = {
      name: 'وصفة مسح ضوئي - ' + new Date().toLocaleDateString('ar-EG'),
      meds: meds.map((m) => ({
        name: m.name ?? '',
        dose: m.dose ?? '',
        frequency: m.frequency ?? '',
        duration: m.duration ?? '',
      })),
    };
    this.showManualModal.set(true);
    this.lockBodyScroll();
  }
  private extractMedsFromImage(base64: string, mediaType: string) {
    return from(
      fetch('https://api.anthropic.com/v1/messages', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          model: 'claude-sonnet-4-6',
          max_tokens: 1000,
          messages: [
            {
              role: 'user',
              content: [
                {
                  type: 'image',
                  source: { type: 'base64', media_type: mediaType, data: base64 },
                },
                {
                  type: 'text',
                  text: `أنت نظام OCR طبي متخصص. استخرج كل الأدوية من هذه الوصفة الطبية.
أعد الرد فقط كـ JSON بهذا الشكل بدون أي نص إضافي أو markdown:
[
  {
    "name": "اسم الدواء التجاري",
    "dose": "الجرعة",
    "frequency": "التكرار",
    "duration": "المدة",
    "chemicalName": "الاسم الكيميائي",
    "registryCode": "رمز التسجيل إن وجد",
    "warning": "أي تحذير أو تعارض محتمل أو null"
  }
]
إذا لم تجد وصفة طبية واضحة، أعد مصفوفة فارغة [].`,
                },
              ],
            },
          ],
        }),
      }),
    ).pipe(
      switchMap((response) => from(response.json())),
      map((data: any) => {
        const text = data.content?.find((b: any) => b.type === 'text')?.text ?? '[]';
        const clean = text.replace(/```json|```/g, '').trim();
        return JSON.parse(clean) as ScannedMed[];
      }),
      catchError(() => from([this.getMockMeds()])),
    );
  }

  private fileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        const result = reader.result as string;
        resolve(result.split(',')[1]);
      };
      reader.onerror = reject;
      reader.readAsDataURL(file);
    });
  }

  private getMockMeds(): ScannedMed[] {
    return [
      {
        name: 'بانادول اكسترا',
        dose: '٥٠٠ ملغ - قرص واحد',
        frequency: '٣ مرات يومياً',
        duration: 'لمدة ٥ أيام',
        chemicalName: 'Paracetamol + Caffeine',
        registryCode: 'EDA-REG-109283-PAN-01',
        warning: undefined,
      },
      {
        name: 'ميتفورمين',
        dose: '٥٠٠ ملغ',
        frequency: 'مرتين يومياً',
        duration: 'مستمر',
        chemicalName: 'Metformin Hydrochloride',
        registryCode: 'EDA-REG-204811-MET-02',
        warning: 'تعارض محتمل مع وارفارين. راجع الطبيب.',
      },
    ];
  }

  reset(): void {
    this.scanned.set(false);
    this.scannedMeds = [];
    this.errorText.set('');
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

  // Manual Modal
  openManualModal() {
    this.manualForm = { name: '', meds: [{ name: '', dose: '', frequency: '', duration: '' }] };
    this.showManualModal.set(true);
    this.lockBodyScroll();
  }

  closeManualModal() {
    this.showManualModal.set(false);
    this.unlockBodyScroll();
  }

  addManualMed() {
    this.manualForm.meds.push({ name: '', dose: '', frequency: '', duration: '' });
  }

  removeManualMed(index: number) {
    this.manualForm.meds.splice(index, 1);
  }

  saveManualPrescription() {
    //   const prescription = {
    //     id: Date.now(),
    //     name: this.manualForm.name || 'وصفة يدوية',
    //     date: new Date().toLocaleDateString('ar-EG'),
    //     source: 'manual',
    //     meds: this.manualForm.meds.filter((m) => m.name.trim()),
    //   };
    //   this.prescriptionService.add(prescription);
    //   this.scanned.set(true);
    //   this.closeManualModal();
    //   this.manualForm = { name: '', meds: [{ name: '', dose: '', frequency: '', duration: '' }] };
  }

  viewPrescription(prescription: any) {
    this.router.navigate(['/prescription-detail', prescription.id]);
  }

  confirmDeletePrescription(prescription: any) {
    this.prescriptionToDelete.set(prescription);
    this.showDeleteConfirm.set(true);
    this.lockBodyScroll();
  }

  deleteConfirmed() {
    // const prescription = this.prescriptionToDelete();
    // if (prescription) {
    //   this.prescriptionService.delete(prescription.id);
    //   this.prescriptionToDelete.set(null);
    //   this.showDeleteConfirm.set(false);
    //   this.unlockBodyScroll();
    //   if (this.prescriptions().length === 0) {
    //     this.scanned.set(false);
    //   }
    // }
  }
}
