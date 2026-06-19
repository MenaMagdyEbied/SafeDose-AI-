import { Component, inject, OnInit, signal } from '@angular/core';
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

  taken = signal(false);
  loading = signal(false);
  errorText = signal('');
  defaultConditions = ['السكري', 'الضغط', 'القلب'];

  pillIcon = Pill;
  activityIcon = Activity;
  cameraIcon = Camera;
  qrCodeIcon = QrCode;
  checkCircleIcon = CircleCheck;
  alertTriangleIcon = TriangleAlert;
  calendarIcon = Calendar;
  showEditModal = signal(false);

  heartIcon = Heart;
  printerIcon = Printer;
  shieldIcon = Shield;

  editIcon = SquarePen;
  trashIcon = Trash2;
  plusIcon = Plus;
  xIcon = X;

  editMeds = signal<{ name: string; dose: string; frequency: string }[]>([]);

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

  medications = signal<{ name: string; dose: string; frequency: string }[]>([]);
  currentPatientId = signal<number | null>(null);

  get user() {
    return {
      phone: '+201099999999',
      name: 'دعاء أشرف',
      age: 30,
      conditions: ['السكري', 'الضغط', 'القلب'],
      allergies: 'لا يوجد',
      doctorName: 'د. مجدي يعقوب',
      subscriptionPlan: 'free',
    };

    // return this.auth.user;
  }

  ngOnInit(): void {
    this.loadPatientData();
  }

  private async loadPatientData(): Promise<void> {
    this.loading.set(true);
    this.errorText.set('');

    try {
      const patients = await this.patientService.getMyPatients();
      const patient = patients[0];
      this.currentPatientId.set(patient?.patientId ?? patient?.id ?? null);

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
  openEditModal() {
    this.editMeds.set(this.medications().map((m) => ({ ...m })));
    this.showEditModal.set(true);
    document.body.style.overflow = 'hidden';
  }
  closeEditModal() {
    this.showEditModal.set(false);
    document.body.style.overflow = '';
  }

  addMed() {
    this.editMeds.update((meds) => [...meds, { name: '', dose: '', frequency: '' }]);
  }

  removeMed(index: number) {
    this.editMeds.update((meds) => meds.filter((_, i) => i !== index));
  }

  updateEditMed(index: number, field: 'name' | 'dose' | 'frequency', value: string) {
    this.editMeds.update((meds) =>
      meds.map((med, i) => (i === index ? { ...med, [field]: value } : med)),
    );
  }

  saveMeds() {
    this.medications.set(this.editMeds().filter((m) => m.name.trim()));
    this.closeEditModal();
    // TODO: PUT /api/user/medications
  }
}
