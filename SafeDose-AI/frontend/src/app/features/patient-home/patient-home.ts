import { Component, inject, signal } from '@angular/core';
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
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-patient-home',
  imports: [LucideAngularModule, RouterLink, FormsModule],
  templateUrl: './patient-home.html',
  styleUrl: './patient-home.css',
})
export class PatientHome {
  protected readonly router = inject(Router);
  private readonly auth = inject(Auth);
  taken = signal(false);
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

  medications = signal<{ name: string; dose: string; frequency: string }[]>([
    { name: 'جلوكوفاج', dose: '٥٠٠ ملغ', frequency: 'مرتان يومياً' },
    { name: 'كونكور', dose: '٥ ملغ', frequency: 'مرة مساءً' },
    { name: 'وارفارين', dose: '٥ ملغ', frequency: 'مرة يومياً' },
  ]);

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
