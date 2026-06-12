import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  Activity,
  AlertTriangle,
  BookOpen,
  ChevronRight,
  LucideAngularModule,
  Pill,
  QrCode,
  Search,
  Sparkles,
  TriangleAlert,
  X,
} from 'lucide-angular';
import { Interaction } from '../../core/services/interaction';

@Component({
  selector: 'app-interaction-checker',
  imports: [FormsModule, LucideAngularModule],
  templateUrl: './interaction-checker.html',
  styleUrl: './interaction-checker.css',
})
export class InteractionChecker {
  private readonly router = inject(Router);
  private readonly interaction = inject(Interaction);
  private readonly cdr = inject(ChangeDetectorRef);
  scanned = false;
  videoStream: MediaStream | null = null;
  showCamera = false;
  searchWord = '';
  resultsOpen = false;
  loading = false;
  selectedMeds: string[] = [];

  pillIcon = Pill;
  searchIcon = Search;
  xIcon = X;
  bookOpenIcon = BookOpen;
  qrCodeIcon = QrCode;
  sparklesIcon = Sparkles;
  activityIcon = Activity;
  alertTriangleIcon = TriangleAlert;
  chevronRightIcon = ChevronRight;

  get filteredDrugs(): string[] {
    return this.interaction.searchDrugs(this.searchWord);
  }

  onSearchChange(val: string): void {
    this.searchWord = val;
    this.resultsOpen = val.length > 0 || true;
  }

  addMed(med: string): void {
    const clean = med.split(' (')[0].trim();
    if (this.selectedMeds.includes(clean) || this.selectedMeds.length >= 6) return;
    this.selectedMeds = [...this.selectedMeds, clean];
    this.searchWord = '';
    this.resultsOpen = false;
  }

  removeMed(index: number): void {
    this.selectedMeds = this.selectedMeds.filter((_, i) => i !== index);
  }

  loadFromProfile(): void {
    this.selectedMeds = ['ميتفورمين', 'وارفارين'];
  }

  voiceInput(): void {
    const SpeechRecognition =
      (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;

    if (!SpeechRecognition) {
      alert('متصفحك مش بيدعم التعرف على الصوت');
      return;
    }

    const recognition = new SpeechRecognition();
    recognition.lang = 'ar-EG';
    recognition.continuous = false;
    recognition.interimResults = false;

    recognition.onstart = () => {
      window.alert('بدأ التسجيل...');
    };

    recognition.onresult = (event: any) => {
      const transcript = event.results[0][0].transcript;
      this.searchWord = transcript;
      this.resultsOpen = true;
      this.cdr.detectChanges();
    };
    recognition.onerror = (event: any) => {
      console.error('خطأ في التسجيل:', event.error);
    };

    recognition.start();
  }

  async runCheck(): Promise<void> {
    this.loading = true;
    const result = await this.interaction.checkInteractions(this.selectedMeds);
    sessionStorage.setItem('lastCheckedFlowOutput', JSON.stringify(result));
    this.loading = false;
    this.router.navigate(['/interaction-results']);
  }

  async scanBarcode(): Promise<void> {
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
  handleFile(file: File): void {
    if (file) this.scanned = true;
    this.cdr.detectChanges();
  }
}
