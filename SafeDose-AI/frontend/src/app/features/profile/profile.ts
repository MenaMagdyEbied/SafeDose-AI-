import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  Camera,
  Clock,
  Heart,
  LucideAngularModule,
  Pill,
  Plus,
  Save,
  Shield,
  SquarePen,
  Stethoscope,
  Trash2,
  TriangleAlert,
  User,
  X,
} from 'lucide-angular';
import { firstValueFrom } from 'rxjs';
import { UserProfileData } from '../../core/models/user-profile';
import { HealthProfile } from '../../core/models';
import { UserProfile } from '../../core/auth/services/user-profile';
import { PatientService, PatientPayload } from '../../core/services/patient';
import { Medications } from '../../core/services/medications';
import { Subscription } from '../../core/services/subscription';
import { Auth } from '../../core/auth/services/auth';

@Component({
  selector: 'app-profile',
  imports: [LucideAngularModule, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {
  userIcon = User;
  heartIcon = Heart;
  alertTriangleIcon = TriangleAlert;
  stethoscopeIcon = Stethoscope;
  cameraIcon = Camera;
  saveIcon = Save;
  pillIcon = Pill;
  shieldIcon = Shield;
  clockIcon = Clock;
  plusIcon = Plus;
  editIcon = SquarePen;
  trashIcon = Trash2;
  xIcon = X;

  editMode = signal(false);
  loading = signal(true);
  saving = signal(false);
  errorText = signal('');
  successText = signal('');

  allConditions = ['السكري', 'ارتفاع ضغط الدم', 'الربو', 'أمراض القلب', 'الحساسية', 'أخرى'];
  bloodTypes = ['A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-'];
  genders = ['ذكر', 'أنثى'];

  profile: HealthProfile = {
    id: '',
    fullName: '',
    phone: '',
    email: '',
    age: null,
    gender: '',
    bloodType: '',
    weight: null,
    height: null,
    conditions: [],
    allergies: '',
    emergency: '',
    emergencyName: '',
    doctor: '',
    subscriptionPlan: 'free',
    joinDate: '',
    medications: [],
    lastCheckup: '',
  };

  commonMeds = [
    'بنادول',
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
  newConditionInput = signal('');
  customConditions = signal<string[]>([]);

  private readonly userProfileService = inject(UserProfile);
  private readonly patientService = inject(PatientService);
  private readonly medicationsService = inject(Medications);
  private readonly subscriptionService = inject(Subscription);
  private readonly auth = inject(Auth);
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.fetchProfile();
  }

  async fetchProfile(): Promise<void> {
    this.loading.set(true);
    this.errorText.set('');
    try {
      const [accountResult, patientsResult, subResult] = await Promise.allSettled([
        firstValueFrom(this.userProfileService.getUserProfile()) as Promise<UserProfileData>,
        this.patientService.getMyPatients(),
        this.subscriptionService.refresh(),
      ]);

      if (accountResult.status === 'fulfilled') {
        const u = accountResult.value;
        this.profile.id = u.id || u.patientId || '';
        this.profile.fullName = u.name || u.userName || '';
        this.profile.email = u.email || '';
        this.profile.phone = u.phone || '';
      }

      if (patientsResult.status === 'fulfilled' && patientsResult.value.length > 0) {
        const p = patientsResult.value[0] as any;
        const currentPatientId = this.patientService.currentPatientId;
        if (!this.profile.fullName) this.profile.fullName = p.fullName || '';
        this.profile.age = this.calcAge(p.dateOfBirth);
        this.profile.gender = this.genderLabel(p.gender);
        this.profile.bloodType = p.bloodType || '';
        this.profile.conditions = this.splitTags(p.chronicConditions);
        this.profile.allergies = this.splitTags(p.allergies).join(' - ');
        this.profile.joinDate = this.formatArabicDate(p.createdAt);

        if (currentPatientId != null) {
          try {
            const meds = await this.medicationsService.getByPatient(currentPatientId);
            this.profile.medications = (meds || []).map((m: any) => ({
              name: m.drugName || m.name || '',
              dose: m.dose || '',
              frequency: this.frequencyLabel(m.frequency),
              startDate: this.formatArabicDate(m.startDate),
            }));
          } catch {
            this.profile.medications = [];
          }
        }
      }

      if (subResult.status === 'fulfilled' && subResult.value) {
        const sub = subResult.value;
        if (sub.tierCode === 'premium-monthly' || sub.tierCode === 'premium-annual') {
          this.profile.subscriptionPlan = 'pro';
        } else {
          this.profile.subscriptionPlan = 'free';
        }
      }
    } catch (err: any) {
      this.errorText.set((err && err.error && err.error.message) || 'تعذر تحميل الملف الشخصي');
    } finally {
      this.loading.set(false);
    }
  }

  toggleCondition(cond: string): void {
    const idx = this.profile.conditions.indexOf(cond);
    if (idx === -1) this.profile.conditions.push(cond);
    else this.profile.conditions.splice(idx, 1);
  }

  async save(): Promise<void> {
    this.saving.set(true);
    this.errorText.set('');
    this.successText.set('');
    try {
      const tasks: Promise<unknown>[] = [];
      if (this.profile.fullName) {
        tasks.push(
          firstValueFrom(this.userProfileService.updateName({ name: this.profile.fullName })),
        );
      }
      if (this.profile.email) {
        tasks.push(
          firstValueFrom(this.userProfileService.updateEmail({ email: this.profile.email })),
        );
      }
      if (this.profile.phone) {
        tasks.push(
          firstValueFrom(this.userProfileService.updatePhone({ phone: this.profile.phone })),
        );
      }
      const currentPatientId = this.patientService.currentPatientId;
      if (currentPatientId != null) {
        const payload: PatientPayload = {
          fullName: this.profile.fullName || 'مريض',
          dateOfBirth: this.ageToDateOfBirth(this.profile.age),
          gender: this.genderCode(this.profile.gender),
          bloodType: this.profile.bloodType || '',
          chronicConditions: this.profile.conditions || [],
          allergies: this.profile.allergies
            ? this.profile.allergies
                .split('-')
                .map((s: string) => s.trim())
                .filter(Boolean)
            : [],
        };
        tasks.push(this.patientService.updatePatient(currentPatientId, payload));
      }
      await Promise.all(tasks);
      this.successText.set('تم حفظ التعديلات');
      this.editMode.set(false);
      this.auth.updateProfile({
        name: this.profile.fullName,
        email: this.profile.email,
        phone: this.profile.phone,
      });
    } catch (err: any) {
      this.errorText.set((err && err.error && err.error.message) || 'فشل حفظ التعديلات');
    } finally {
      this.saving.set(false);
    }
  }

  get planLabel(): string {
    const plans: Record<string, string> = {
      free: 'مجاني',
      pro: 'SafeDose Pro',
      family: 'خطة العيلة',
    };
    return plans[this.profile.subscriptionPlan] || 'مجاني';
  }

  get planColor(): string {
    const colors: Record<string, string> = {
      free: 'bg-surface-container text-outline',
      pro: 'bg-secondary-container text-on-secondary-container',
      family: 'bg-tertiary-container text-on-tertiary-container',
    };
    return colors[this.profile.subscriptionPlan] || '';
  }

  addMedication(): void {
    this.profile.medications.push({
      name: '',
      dose: '',
      frequency: '',
      startDate: new Date().toLocaleDateString('ar-EG'),
    });
  }

  removeMedication(index: number): void {
    this.profile.medications.splice(index, 1);
  }

  addCustomCondition(): void {
    const val = this.newConditionInput().trim();
    if (val && !this.customConditions().includes(val) && !this.profile.conditions.includes(val)) {
      this.customConditions.update((conds) => [...conds, val]);
      this.profile.conditions.push(val);
    }
    this.newConditionInput.set('');
  }

  removeCustomCondition(cond: string): void {
    this.customConditions.update((conds) => conds.filter((c) => c !== cond));
    this.profile.conditions = this.profile.conditions.filter((c: any) => c !== cond);
  }

  onMedInput(med: any, event: Event): void {
    const val = (event.target as HTMLInputElement).value.trim();
    if (val.length < 1) {
      this.currentSuggestionsMap.delete(med);
      return;
    }
    this.currentSuggestionsMap.set(
      med,
      this.commonMeds.filter((m) => m.includes(val)),
    );
  }

  activeSuggestions(med: any): string[] {
    return this.currentSuggestionsMap.get(med) || [];
  }

  selectSuggestion(med: any, name: string): void {
    med.name = name;
    this.currentSuggestionsMap.delete(med);
  }

  clearSuggestions(): void {
    setTimeout(() => this.currentSuggestionsMap.clear(), 150);
  }

  // helpers
  private calcAge(dob: string | null | undefined): number | null {
    if (!dob) return null;
    const birth = new Date(dob);
    if (Number.isNaN(birth.getTime())) return null;
    const today = new Date();
    let age = today.getFullYear() - birth.getFullYear();
    const m = today.getMonth() - birth.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < birth.getDate())) age--;
    return age >= 0 ? age : null;
  }

  private ageToDateOfBirth(age: number | null): string {
    if (age == null || !Number.isFinite(age) || age < 0) {
      return new Date().toISOString().slice(0, 10);
    }
    const year = new Date().getFullYear() - Math.floor(age);
    return year + '-01-01';
  }

  private genderLabel(g: number | string | null | undefined): string {
    const n = typeof g === 'string' ? parseInt(g, 10) : g;
    if (n === 1) return 'ذكر';
    if (n === 2) return 'أنثى';
    return '';
  }

  private genderCode(label: string): number {
    if (label === 'ذكر') return 1;
    if (label === 'أنثى') return 2;
    return 0;
  }

  private splitTags(csv: any): string[] {
    if (!csv) return [];
    if (Array.isArray(csv)) return csv.filter(Boolean);
    return String(csv)
      .split(',')
      .map((s) => s.trim())
      .filter(Boolean);
  }

  private frequencyLabel(freq: number | null | undefined): string {
    if (!freq || freq < 1) return '';
    if (freq === 1) return 'مرة يومياً';
    if (freq === 2) return 'مرتين يومياً';
    return freq + ' مرات يومياً';
  }

  private formatArabicDate(d: string | null | undefined): string {
    if (!d) return '';
    const dt = new Date(d);
    if (Number.isNaN(dt.getTime())) return '';
    return dt.toLocaleDateString('ar-EG');
  }
}
