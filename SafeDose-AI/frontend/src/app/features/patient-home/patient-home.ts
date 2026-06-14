import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
  Activity,
  Calendar,
  Camera,
  CircleCheck,
  LucideAngularModule,
  Pill,
  QrCode,
  TriangleAlert,
} from 'lucide-angular';
import { Auth } from '../../core/auth/services/auth';

@Component({
  selector: 'app-patient-home',
  imports: [LucideAngularModule,RouterLink],
  templateUrl: './patient-home.html',
  styleUrl: './patient-home.css',
})
export class PatientHome {
  protected readonly router = inject(Router);
  private readonly auth = inject(Auth);
  taken = false;
  defaultConditions = ['السكري', 'الضغط', 'القلب'];

  pillIcon = Pill;
  activityIcon = Activity;
  cameraIcon = Camera;
  qrCodeIcon = QrCode;
  checkCircleIcon = CircleCheck;
  alertTriangleIcon = TriangleAlert;
  calendarIcon = Calendar;

  get user() {
    return {
      phone: '+201099999999',
      name:  'دعاء أشرف',
      age: 30,
      conditions: ['السكري', 'الضغط', 'القلب'],
      allergies: 'لا يوجد',
      doctorName: 'د. مجدي يعقوب',
      subscriptionPlan: 'free',
    };

    // return this.auth.user;
  }

  takeDose(): void {
    this.taken = true;
  }

  showSymptomsReport(): void {
    window.alert('تم فتح تقرير الأعراض.');
  }
}
