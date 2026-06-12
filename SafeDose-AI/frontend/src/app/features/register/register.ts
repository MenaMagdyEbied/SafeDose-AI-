import { Component, inject } from '@angular/core';
import {
  ArrowRight,
  FileCheck,
  Heart,
  LucideAngularModule,
  Minus,
  Pill,
  Plus,
  Shield,
  TriangleAlert,
  User,
} from 'lucide-angular';
import { Router } from '@angular/router';
import { Auth } from '../../core/auth/services/auth';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register',
  imports: [FormsModule, LucideAngularModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private readonly router = inject(Router);
  private readonly auth = inject(Auth);
  screen: 'welcome' | 'phone' | 'otp' | 'consent' | 'wizard' = 'welcome';
  phone = '01099999999';
  digits: string[] = Array(6).fill('');
  errorText = '';
  loading = false;
  consent1 = false;
  consent2 = false;
  wizardStep = 1;
  wizardName = '';
  wizardAge = 65;
  wizardConditions: string[] = [];
  wizardAllergies = '';
  wizardDoctor = '';

  famousConditions = [
    { id: 'السكري', label: 'السكري (Diabetes)' },
    { id: 'الضغط', label: 'الضغط (Hypertension)' },
    { id: 'القلب', label: 'القلب (Heart Disease)' },
    { id: 'الكوليسترول', label: 'الكوليسترول' },
    { id: 'الربو', label: 'الربو (Asthma)' },
    { id: 'الكلى', label: 'الكلى (Kidney)' },
    { id: 'أخرى', label: 'حالة أخرى' },
  ];

  heartIcon = Heart;
  pillIcon = Pill;
  shieldIcon = Shield;
  arrowRightIcon = ArrowRight;
  alertTriangleIcon = TriangleAlert;
  fileCheckIcon = FileCheck;
  plusIcon = Plus;
  minusIcon = Minus;
  userIcon = User;

  goBack(): void {
    if (this.screen === 'welcome') this.router.navigate(['/home']);
    else if (this.screen === 'phone') this.screen = 'welcome';
    else if (this.screen === 'otp') this.screen = 'phone';
    else if (this.screen === 'consent') this.screen = 'otp';
    else if (this.screen === 'wizard') this.screen = 'consent';
  }

  onPhoneChange(val: string): void {
    this.phone = val.replace(/\D/g, '').substring(0, 11);
  }

  sendOtp(): void {
    this.errorText = '';
    this.loading = true;
    setTimeout(() => {
      this.screen = 'otp';
      this.loading = false;
    }, 800);
  }

  onDigitInput(index: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    const val = input.value.replace(/\D/g, '').slice(-1);
    this.digits[index] = val;
    if (val && index < 5) {
      const next = input.nextElementSibling as HTMLInputElement;
      if (next) next.focus();
    }
    const code = this.digits.join('');
    if (code.length === 6 && !this.loading) this.verifyOtp(code);
  }

  onDigitKeydown(index: number, event: KeyboardEvent): void {
    if (event.key === 'Backspace' && !this.digits[index] && index > 0) {
      const prev = (event.target as HTMLInputElement).previousElementSibling as HTMLInputElement;
      if (prev) prev.focus();
    }
  }

  verifyOtp(code: string): void {
    this.loading = true;
    setTimeout(() => {
      if (code === '123456') {
        this.screen = 'consent';
      } else {
        this.errorText = 'الكود غير صحيح، حاول مرة أخرى';
      }
      this.loading = false;
    }, 800);
  }

  resendOtp(): void {
    this.digits = Array(6).fill('');
    this.sendOtp();
  }

  acceptConsent(): void {
    this.screen = 'wizard';
  }

  toggleCondition(id: string): void {
    if (this.wizardConditions.includes(id)) {
      this.wizardConditions = this.wizardConditions.filter((c) => c !== id);
    } else {
      this.wizardConditions = [...this.wizardConditions, id];
    }
  }

  wizardNext(): void {
    if (this.wizardStep < 5) {
      this.wizardStep++;
    } else {
      this.completeWizard();
    }
  }

  wizardBack(): void {
    if (this.wizardStep > 1) this.wizardStep--;
  }

  completeWizard(): void {
    const patient = {
      phone: '+201099999999',
      name: this.wizardName || 'أحمد المنسي',
      age: this.wizardAge,
      conditions:
        this.wizardConditions.length > 0 ? this.wizardConditions : ['السكري', 'الضغط', 'القلب'],
      allergies: this.wizardAllergies || 'لا يوجد',
      doctorName: this.wizardDoctor || 'د. مجدي يعقوب',
      subscriptionPlan: 'free' as const,
    };
    this.auth.login(patient, 'jwt_token_' + Date.now());
    this.router.navigate(['/patient']);
  }

  decrementAge(): void {
    this.wizardAge = Math.max(18, this.wizardAge - 1);
  }

  incrementAge(): void {
    this.wizardAge = Math.min(100, this.wizardAge + 1);
  }
}
