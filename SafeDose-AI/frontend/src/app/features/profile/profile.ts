import { Component, signal } from '@angular/core';
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
import { UserProfileData } from '../../core/models/user-profile';
import { HealthProfile } from '../../core/models';

@Component({
  selector: 'app-profile',
  imports: [LucideAngularModule, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile {
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

  // Mock API response
  private mockApiResponse: HealthProfile = {
    id: 'USR-00412',
    fullName: 'دعاء أحمد محمود',
    phone: '+20 1099 999 999',
    email: 'doaa.ahmed@safedose.ai',
    age: 34,
    gender: 'أنثى',
    bloodType: 'A+',
    weight: 65,
    height: 165,
    conditions: ['السكري', 'ارتفاع ضغط الدم'],
    allergies: 'بنسلين - سلفا',
    emergency: '+20 1011 111 111',
    emergencyName: 'أحمد محمود (الزوج)',
    doctor: 'د. محمد السيد - مستشفى الشفاء',
    subscriptionPlan: 'pro',
    joinDate: '٢٠٢٤/٠٣/١٥',
    lastCheckup: '٢٠٢٥/٠٥/٢٠',
    medications: [
      { name: 'ميتفورمين', dose: '٥٠٠ ملغ', frequency: 'مرتين يومياً', startDate: '٢٠٢٣/٠١/١٠' },
      { name: 'أملوديبين', dose: '٥ ملغ', frequency: 'مرة يومياً', startDate: '٢٠٢٣/٠٦/٠١' },
      { name: 'أسبرين', dose: '٨١ ملغ', frequency: 'مرة يومياً', startDate: '٢٠٢٤/٠١/٠١' },
    ],
  };

  ngOnInit(): void {
    this.fetchProfile();
  }

  fetchProfile(): void {
    this.loading.set(true);
    this.profile = { ...this.mockApiResponse };
    this.loading.set(false);
  }

  toggleCondition(cond: string): void {
    const idx = this.profile.conditions.indexOf(cond);
    if (idx === -1) this.profile.conditions.push(cond);
    else this.profile.conditions.splice(idx, 1);
  }

  save(): void {
    // TODO: PUT /api/user/profile
    console.log('Saving profile:', this.profile);
    this.editMode.set(false);
  }

  get planLabel(): string {
    const plans: Record<string, string> = {
      free: 'مجاني',
      pro: 'SafeDose Pro ⭐',
      family: 'خطة العيلة 👨‍👩‍👧‍👦',
    };
    return plans[this.profile.subscriptionPlan] ?? 'مجاني';
  }

  get planColor(): string {
    const colors: Record<string, string> = {
      free: 'bg-surface-container text-outline',
      pro: 'bg-secondary-container text-on-secondary-container',
      family: 'bg-tertiary-container text-on-tertiary-container',
    };
    return colors[this.profile.subscriptionPlan] ?? '';
  }

  newConditionInput = signal('');
  customConditions = signal<string[]>([]);

  addMedication() {
    this.profile.medications.push({
      name: '',
      dose: '',
      frequency: '',
      startDate: new Date().toLocaleDateString('ar-EG'),
    });
  }

  removeMedication(index: number) {
    this.profile.medications.splice(index, 1);
  }

  addCustomCondition() {
    const val = this.newConditionInput().trim();
    if (val && !this.customConditions().includes(val) && !this.profile.conditions.includes(val)) {
      this.customConditions.update((conds) => [...conds, val]);
      this.profile.conditions.push(val);
    }
    this.newConditionInput.set('');
  }

  removeCustomCondition(cond: string) {
    this.customConditions.update((conds) => conds.filter((c) => c !== cond));
    this.profile.conditions = this.profile.conditions.filter((c: any) => c !== cond);
  }
}
