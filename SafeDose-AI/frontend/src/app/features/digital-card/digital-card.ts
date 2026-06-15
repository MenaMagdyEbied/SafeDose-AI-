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

 

  
}
