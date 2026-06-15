import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
  Heart,
  Pill,
  QrCode,
  Printer,
  Shield,
  Activity,
  LucideAngularModule,
  LucideIconProvider,
  LUCIDE_ICONS,
  SquarePen,
  X,
  Plus,
  Trash2,
} from 'lucide-angular';
import { Auth } from '../../core/auth/services/auth';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-digital-card',
  imports: [LucideAngularModule, RouterLink, FormsModule],
  templateUrl: './digital-card.html',
  styleUrl: './digital-card.css',
})
export class DigitalCard {
  protected readonly router = inject(Router);
  private readonly auth = inject(Auth);

  heartIcon = Heart;
  pillIcon = Pill;
  qrCodeIcon = QrCode;
  printerIcon = Printer;
  shieldIcon = Shield;

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

  printCard(): void {
    window.print();
  }
  editIcon = SquarePen;
  trashIcon = Trash2;
  plusIcon = Plus;
  xIcon = X;

  showEditModal = false;
  editMeds: { name: string; dose: string; frequency: string }[] = [];

  medications: { name: string; dose: string; frequency: string }[] = [
    { name: 'جلوكوفاج', dose: '٥٠٠ ملغ', frequency: 'مرتان يومياً' },
    { name: 'كونكور', dose: '٥ ملغ', frequency: 'مرة مساءً' },
    { name: 'وارفارين', dose: '٥ ملغ', frequency: 'مرة يومياً' },
  ];

  openEditModal() {
    this.editMeds = this.medications.map((m) => ({ ...m }));
    this.showEditModal = true;
    document.body.style.overflow = 'hidden';
  }

  closeEditModal() {
    this.showEditModal = false;
    document.body.style.overflow = '';
  }

  addMed() {
    this.editMeds.push({ name: '', dose: '', frequency: '' });
  }

  removeMed(index: number) {
    this.editMeds.splice(index, 1);
  }

  saveMeds() {
    this.medications = this.editMeds.filter((m) => m.name.trim());
    this.closeEditModal();
    // TODO: PUT /api/user/medications
  }
}
