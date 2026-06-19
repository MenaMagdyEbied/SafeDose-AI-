import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  Activity,
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
import { EMPTY, from } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { Interaction } from '../../core/services/interaction';
import { Medications } from '../../core/services/medications';
import { PatientService } from '../../core/services/patient';
import { DrugSearchResult } from '../../core/models';
interface SelectedMed {
  drugCatalogId: number;
  name: string;
}

@Component({
  selector: 'app-interaction-checker',
  imports: [FormsModule, LucideAngularModule],
  templateUrl: './interaction-checker.html',
  styleUrl: './interaction-checker.css',
})
export class InteractionChecker {
  private readonly router = inject(Router);
  private readonly interaction = inject(Interaction);
  private readonly medicationsService = inject(Medications);
  private readonly patientService = inject(PatientService);
  private readonly destroyRef = inject(DestroyRef);

  scanned = signal(false);
  videoStream = signal<MediaStream | null>(null);
  showCamera = signal(false);
  searchWord = signal('');
  resultsOpen = signal(false);
  loading = signal(false);
  selectedMeds = signal<SelectedMed[]>([]);

  // قائمة "من ملف أدويتي"
  showProfileMeds = signal(false);
  profileMedsLoading = signal(false);
  profileMeds = signal<{ drugCatalogId: number; name: string; checked: boolean }[]>([]);

  pillIcon = Pill;
  searchIcon = Search;
  xIcon = X;
  bookOpenIcon = BookOpen;
  qrCodeIcon = QrCode;
  sparklesIcon = Sparkles;
  activityIcon = Activity;
  alertTriangleIcon = TriangleAlert;
  chevronRightIcon = ChevronRight;

  private currentPatientId: number | null = null;

  readonly filteredDrugs = computed(() => this.interaction.searchResults());

  ngOnInit(): void {
    // نجيب الـ patientId الأساسي بمجرد ما الصفحة تفتح، هنحتاجه وقت الفحص والتحميل من البروفايل
    this.patientService
      .getMyPatients()
      .then((patients) => {
        this.currentPatientId = patients[0]?.patientId ?? patients[0]?.id ?? null;
      })
      .catch(() => {
        this.currentPatientId = null;
      });
  }

  onSearchChange(val: string): void {
    this.searchWord.set(val);
    this.resultsOpen.set(true);

    if (!val.trim()) {
      this.interaction.searchResults.set([]);
      return;
    }

    from(this.interaction.searchDrugs(val))
      .pipe(
        catchError(() => EMPTY),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  addMed(drug: DrugSearchResult): void {
    if (
      this.selectedMeds().some((m) => m.drugCatalogId === drug.drugCatalogId) ||
      this.selectedMeds().length >= 6
    ) {
      return;
    }
    this.selectedMeds.update((list) => [
      ...list,
      { drugCatalogId: drug.drugCatalogId, name: drug.commercialNameAr || drug.commercialNameEn },
    ]);
    this.searchWord.set('');
    this.resultsOpen.set(false);
  }

  removeMed(index: number): void {
    this.selectedMeds.update((list) => list.filter((_, i) => i !== index));
  }

  /** فتح قائمة "من ملف أدويتي" مع checkbox لكل دواء */
  loadFromProfile(): void {
    this.showProfileMeds.set(true);

    if (!this.currentPatientId) {
      this.profileMeds.set([]);
      return;
    }

    this.profileMedsLoading.set(true);

    from(this.medicationsService.getByPatient(this.currentPatientId))
      .pipe(
        catchError(() => EMPTY),
        finalize(() => this.profileMedsLoading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((meds) => {
        this.profileMeds.set(
          meds.map((m) => ({
            drugCatalogId: m.drugCatalogId,
            name: m.drugName,
            checked: this.selectedMeds().some((s) => s.drugCatalogId === m.drugCatalogId),
          })),
        );
      });
  }

  closeProfileMeds(): void {
    this.showProfileMeds.set(false);
  }

  /** بمجرد ما اليوزر يدوس على checkbox، الدوا يتضاف أو يتشال من selectedMeds على طول */
  toggleProfileMed(med: { drugCatalogId: number; name: string; checked: boolean }): void {
    const isCurrentlyChecked = this.selectedMeds().some(
      (m) => m.drugCatalogId === med.drugCatalogId,
    );

    if (isCurrentlyChecked) {
      this.selectedMeds.update((list) => list.filter((m) => m.drugCatalogId !== med.drugCatalogId));
    } else {
      if (this.selectedMeds().length >= 6) return;
      this.selectedMeds.update((list) => [
        ...list,
        { drugCatalogId: med.drugCatalogId, name: med.name },
      ]);
    }

    this.profileMeds.update((list) =>
      list.map((m) =>
        m.drugCatalogId === med.drugCatalogId ? { ...m, checked: !isCurrentlyChecked } : m,
      ),
    );
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
      this.onSearchChange(transcript);
    };
    recognition.onerror = (event: any) => {
      console.error('خطأ في التسجيل:', event.error);
    };

    recognition.start();
  }

  runCheck(): void {
    if (!this.currentPatientId) return;

    this.loading.set(true);

    const payload = {
      drugCatalogIds: this.selectedMeds().map((m) => m.drugCatalogId),
      patientId: this.currentPatientId,
    };

    from(this.interaction.checkInteractions(payload))
      .pipe(
        catchError(() => EMPTY),
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((result) => {
        sessionStorage.setItem('lastCheckedFlowOutput', JSON.stringify(result));
        this.router.navigate(['/interaction-results']);
      });
  }

  scanBarcode(): void {
    this.showCamera.set(true);

    from(
      navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' },
      }),
    )
      .pipe(
        catchError(() => {
          this.showCamera.set(false);
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((stream) => {
        this.videoStream.set(stream);

        setTimeout(() => {
          const video = document.getElementById('cameraFeed') as HTMLVideoElement;
          if (video) {
            video.srcObject = this.videoStream();
          }
        }, 100);
      });
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
    this.videoStream()
      ?.getTracks()
      .forEach((t) => t.stop());
    this.videoStream.set(null);
    this.showCamera.set(false);
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
    if (file) this.scanned.set(true);
  }
}
