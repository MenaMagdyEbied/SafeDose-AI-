import { Component, DestroyRef, effect, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
  Activity,
  Calendar,
  Camera,
  CircleCheck,
  Heart,
  LucideAngularModule,
  Pill,
  Plus,
  Printer,
  QrCode,
  Shield,
  SquarePen,
  Trash2,
  TriangleAlert,
  X,
} from 'lucide-angular';
import { Auth } from '../../core/auth/services/auth';
import { Medications } from '../../core/services/medications';
import { PatientService } from '../../core/services/patient';
import { AddMedication } from '../../shared/components/add-medication/add-medication';
import { Subscription as SubscriptionService } from '../../core/services/subscription';

@Component({
  selector: 'app-patient-home',
  imports: [LucideAngularModule, RouterLink, AddMedication],
  templateUrl: './patient-home.html',
  styleUrl: './patient-home.css',
})
export class PatientHome implements OnInit {
  protected readonly router = inject(Router);
  private readonly auth = inject(Auth);
  private readonly patientService = inject(PatientService);
  private readonly medicationsService = inject(Medications);
  private readonly subscriptionService = inject(SubscriptionService);
  private readonly destroyRef = inject(DestroyRef);

  loading = signal(false);
  errorText = signal('');
  successText = signal('');
  nextDose = signal<{
    patientMedicationId: number;
    drugName: string;
    dose: string;
    mealTiming: string;
    timeLabel: string;
    taken: boolean;
  } | null>(null);

  showEditModal = signal(false);

  medications = signal<{ name: string; dose: string; frequency: string }[]>([]);
  currentPatientId = signal<number | null>(null);

  // Icons
  pillIcon = Pill;
  activityIcon = Activity;
  cameraIcon = Camera;
  qrCodeIcon = QrCode;
  checkCircleIcon = CircleCheck;
  alertTriangleIcon = TriangleAlert;
  calendarIcon = Calendar;
  heartIcon = Heart;
  printerIcon = Printer;
  shieldIcon = Shield;
  trashIcon = Trash2;
  plusIcon = Plus;
  xIcon = X;

  get user() {
    const subscription = this.subscriptionService.current();
    const isPaid = this.subscriptionService.hasActivePaidPlan();

    return {
      phone: this.auth.user?.phone || '+201099999999',
      name: this.auth.user?.name || this.auth.user?.userName || 'مستخدم',
      age: 30,
      conditions: ['السكري', 'الضغط', 'القلب'],
      allergies: 'لا يوجد',
      doctorName: 'د. مجدي يعقوب',
      subscriptionPlan: isPaid ? 'pro' : subscription?.tierCode === 'free' ? 'free' : 'pro',
    };
  }

  constructor() {
    effect(() => {
      const patientId = this.patientService.currentPatientId;
      if (patientId != null && patientId !== this.currentPatientId()) {
        this.currentPatientId.set(patientId);
        void this.loadPatientData();
      }
    });
  }

  ngOnInit(): void {
    void this.subscriptionService.refresh();
    this.loadPatientData();
  }

  showSymptomsReport(): void {
    window.alert('تم فتح تقرير الأعراض.');
  }

  openEditModal(): void {
    this.showEditModal.set(true);
    document.body.style.overflow = 'hidden';
  }

  closeEditModal(): void {
    this.showEditModal.set(false);
    document.body.style.overflow = '';
  }

  onMedicationSaved(): void {
    this.closeEditModal();
    this.successText.set('تمت إضافة الدواء بنجاح');
    this.loadPatientData();
  }

  onMedicationCancelled(): void {
    this.closeEditModal();
  }

  private async loadPatientData(): Promise<void> {
    this.loading.set(true);
    this.errorText.set('');

    try {
      const patientId =
        this.patientService.currentPatientId ?? (await this.patientService.getPrimaryPatientId());
      this.currentPatientId.set(patientId);

      if (!this.currentPatientId()) {
        this.errorText.set('تعذر تحديد المريض الحالي.');
        return;
      }

      const meds = await this.medicationsService.getByPatient(this.currentPatientId()!);
      this.medications.set(
        meds.map((item) => ({
          name: item.drugName,
          dose: item.dose ?? item.drugDose ?? '—',
          frequency: item.frequency ? `${item.frequency} مرة يومياً` : '—',
        })),
      );

      this.computeNextDose(meds);
    } catch {
      this.errorText.set('تعذر تحميل الأدوية من الخادم.');
    } finally {
      this.loading.set(false);
    }
  }

  private computeNextDose(meds: any[]): void {
    const nowMin = new Date().getHours() * 60 + new Date().getMinutes();
    let best: { tMin: number; m: any; t: string } | null = null;

    for (const m of meds) {
      const times: string[] =
        Array.isArray(m.times) && m.times.length > 0 ? m.times : this.fallbackTimes(m.frequency);

      for (const t of times) {
        const tMin = this.parseMin(t);
        if (tMin == null || tMin <= nowMin) continue; // بس المواعيد الجاية

        if (best === null || tMin < best.tMin) {
          best = { tMin, m, t };
        }
      }
    }

    if (!best) {
      this.nextDose.set(null); // مفيش معاد جاي النهاردة
      return;
    }

    this.nextDose.set({
      patientMedicationId: best.m.patientMedicationId,
      drugName: best.m.drugName || best.m.name || 'دواء',
      dose: best.m.dose ?? best.m.drugDose ?? '',
      mealTiming: best.m.mealTimingArabic || '',
      timeLabel: this.fmt(best.tMin),
      taken: false,
    });
  }

  private parseMin(t: string): number | null {
    const m = /^(\d{1,2}):(\d{2})/.exec(t || '');
    if (!m) return null;
    return parseInt(m[1], 10) * 60 + parseInt(m[2], 10);
  }

  private fmt(mins: number): string {
    const h = Math.floor(mins / 60);
    const m = mins % 60;
    const ampm = h >= 12 ? 'م' : 'ص';
    const h12 = h === 0 ? 12 : h > 12 ? h - 12 : h;
    return `${h12}:${m.toString().padStart(2, '0')} ${ampm}`;
  }

  private fallbackTimes(freq: number | null | undefined): string[] {
    if (!freq || freq < 1) return [];
    if (freq === 1) return ['09:00'];
    if (freq === 2) return ['09:00', '21:00'];
    if (freq === 3) return ['09:00', '15:00', '21:00'];
    if (freq === 4) return ['08:00', '12:00', '16:00', '20:00'];
    return Array.from({ length: freq }, (_, i) => {
      const slot = Math.round((24 / freq) * i + 8) % 24;
      return `${slot.toString().padStart(2, '0')}:00`;
    });
  }

  takeDose(): void {
    const nd = this.nextDose();
    if (!nd) return;
    this.nextDose.set({ ...nd, taken: true });
    // لو عايزة تبعتي API call هنا كمان زي notifications.ts، قوليلي وأضيفه
  }
}
