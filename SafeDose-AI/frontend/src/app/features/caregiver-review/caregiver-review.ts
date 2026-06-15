import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
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
import { FormsModule } from '@angular/forms';
import { Prescription } from '../../core/services/prescription';

@Component({
  selector: 'app-caregiver-review',
  imports: [LucideAngularModule, RouterLink, FormsModule],
  templateUrl: './caregiver-review.html',
  styleUrl: './caregiver-review.css',
})
export class CaregiverReview implements OnInit {
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly prescriptionService = inject(Prescription);

  scanned = false;
  loading = false;
  errorText = '';
  scannedMeds: ScannedMed[] = [];

  cameraIcon = Camera;
  uploadIcon = Upload;
  rotateCcwIcon = RotateCcw;
  alertTriangleIcon = TriangleAlert;
  checkCircleIcon = CircleCheck;
  videoStream: MediaStream | null = null;
  showCamera = false;
  plusIcon = Plus;
  trashIcon = Trash2;
  xIcon = X;
  eyeIcon = Eye;

  // Delete
  showDeleteConfirm = false;
  prescriptionToDelete: any = null;

  // Manual Modal
  showManualModal = false;
  manualForm: {
    name: string;
    meds: { name: string; dose: string; frequency: string; duration: string }[];
  } = {
    name: '',
    meds: [{ name: '', dose: '', frequency: '', duration: '' }],
  };

  ngOnInit() {
    if (this.prescriptions.length > 0) {
      this.scanned = true;
    }
  }

  get prescriptions() {
    return this.prescriptionService.prescriptions;
  }

  // Camera
  async openCamera(): Promise<void> {
    try {
      this.showCamera = true;
      this.videoStream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' },
      });
      setTimeout(() => {
        const video = document.getElementById('cameraFeed') as HTMLVideoElement;
        if (video) video.srcObject = this.videoStream;
      }, 100);
    } catch (err) {
      this.showCamera = false;
    }
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

  closeCamera(): void {
    this.videoStream?.getTracks().forEach((t) => t.stop());
    this.videoStream = null;
    this.showCamera = false;
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

  async handleFile(file: File): Promise<void> {
    this.loading = true;
    this.errorText = '';
    this.cdr.detectChanges();

    try {
      const base64 = await this.fileToBase64(file);
      const meds = await this.extractMedsFromImage(base64, file.type);
      this.openReviewModal(meds);
    } catch (err) {
      this.errorText = 'حدث خطأ أثناء تحليل الوصفة. حاول مرة أخرى.';
      this.openReviewModal(this.getMockMeds());
    } finally {
      this.loading = false;
      this.cdr.detectChanges();
    }
  }

  // بيفتح المودال مليان بالأدوية اللي استخرجها الـ agent
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
    this.showManualModal = true;
  }

  private async extractMedsFromImage(base64: string, mediaType: string): Promise<ScannedMed[]> {
    const response = await fetch('https://api.anthropic.com/v1/messages', {
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
    });

    const data = await response.json();
    const text = data.content?.find((b: any) => b.type === 'text')?.text ?? '[]';
    const clean = text.replace(/```json|```/g, '').trim();
    return JSON.parse(clean) as ScannedMed[];
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
    this.scanned = false;
    this.scannedMeds = [];
    this.errorText = '';
  }

  // Manual Modal
  openManualModal() {
    this.manualForm = { name: '', meds: [{ name: '', dose: '', frequency: '', duration: '' }] };
    this.showManualModal = true;
  }

  closeManualModal() {
    this.showManualModal = false;
  }

  addManualMed() {
    this.manualForm.meds.push({ name: '', dose: '', frequency: '', duration: '' });
  }

  removeManualMed(index: number) {
    this.manualForm.meds.splice(index, 1);
  }

  saveManualPrescription() {
    const prescription = {
      id: Date.now(),
      name: this.manualForm.name || 'وصفة يدوية',
      date: new Date().toLocaleDateString('ar-EG'),
      source: 'manual',
      meds: this.manualForm.meds.filter((m) => m.name.trim()),
    };
    this.prescriptionService.add(prescription);
    this.scanned = true;
    this.showManualModal = false;
    this.manualForm = { name: '', meds: [{ name: '', dose: '', frequency: '', duration: '' }] };
  }

  viewPrescription(prescription: any) {
    this.router.navigate(['/prescription-detail', prescription.id]);
  }

  // Delete
  confirmDeletePrescription(prescription: any) {
    this.prescriptionToDelete = prescription;
    this.showDeleteConfirm = true;
  }

  deleteConfirmed() {
    if (this.prescriptionToDelete) {
      this.prescriptionService.delete(this.prescriptionToDelete.id);
      this.prescriptionToDelete = null;
      this.showDeleteConfirm = false;
      if (this.prescriptions.length === 0) {
        this.scanned = false;
      }
    }
  }
}
