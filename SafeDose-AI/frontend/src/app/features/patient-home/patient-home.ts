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

  taken = signal(false);
  loading = signal(false);
  errorText = signal('');
  successText = signal('');

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
      subscriptionPlan: isPaid ? 'pro' : (subscription?.tierCode === 'free' ? 'free' : 'pro'),
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
    } catch {
      this.errorText.set('تعذر تحميل الأدوية من الخادم.');
    } finally {
      this.loading.set(false);
    }
  }

  takeDose(): void {
    this.taken.set(true);
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
}
