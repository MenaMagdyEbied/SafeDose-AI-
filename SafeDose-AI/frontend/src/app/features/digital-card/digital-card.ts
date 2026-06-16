import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { Auth } from '../../core/auth/services/auth';
import { CardData } from '../../core/models/card-data';
import { Card } from '../../shared/components/card/card';
import { ArrowRight } from 'lucide-angular';

import { Heart, Pill, Plus, Printer, QrCode, Shield, SquarePen, Trash2, X } from 'lucide-angular';

@Component({
  selector: 'app-digital-card',
  imports: [LucideAngularModule, RouterLink, FormsModule, Card],
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
  editIcon = SquarePen;
  trashIcon = Trash2;
  plusIcon = Plus;
  xIcon = X;

  arrowRightIcon = ArrowRight;
  get userData() {
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
  get cardData(): CardData {
    return {
      id: '123',
      name: this.userData.name,
      age: this.userData.age,
      medications: [],
      allergies: [this.userData.allergies],
      doctorName: this.userData.doctorName,
      qrUrl: `https://yourdomain.com/card/123`, // ← ده اللي الـ QR هيشاور عليه
    };
  }

  generateQR(): void {
    // توديه لصفحة الـ QR وتبعتيله الـ URL
    this.router.navigate(['/qr'], {
      queryParams: { url: this.cardData.qrUrl },
    });
  }
}
