import { Component } from '@angular/core';
import {
  Camera,
  Clock,
  Heart,
  LucideAngularModule,
  Pill,
  Save,
  Shield,
  Stethoscope,
  TriangleAlert,
  User,
} from 'lucide-angular';
import { FormsModule } from '@angular/forms';
import { UserProfile } from '../../core/models/user-profile';

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

  editMode = false;
  loading = true;

  allConditions = ['السكري', 'ارتفاع ضغط الدم', 'الربو', 'أمراض القلب', 'الحساسية', 'أخرى'];
  bloodTypes = ['A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-'];
  genders = ['ذكر', 'أنثى'];

  profile: UserProfile = {
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

  // Mock API response
  private mockApiResponse: UserProfile = {
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
    this.loading = true;
    // Simulate API call delay
    this.profile = { ...this.mockApiResponse };
    this.loading = false;
  }

  toggleCondition(cond: string): void {
    const idx = this.profile.conditions.indexOf(cond);
    if (idx === -1) this.profile.conditions.push(cond);
    else this.profile.conditions.splice(idx, 1);
  }

  save(): void {
    // TODO: PUT /api/user/profile
    console.log('Saving profile:', this.profile);
    this.editMode = false;
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
}
