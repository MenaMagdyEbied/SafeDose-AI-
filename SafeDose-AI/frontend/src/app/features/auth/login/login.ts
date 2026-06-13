import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ArrowLeft, LucideAngularModule, ShieldCheck, TriangleAlert } from 'lucide-angular';

@Component({
  selector: 'app-login',
  imports: [FormsModule, LucideAngularModule, RouterLink],

  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly router = inject(Router);
  arrowLeftIcon = ArrowLeft;
  shieldCheckIcon = ShieldCheck;
  alertTriangleIcon = TriangleAlert;

  // State
  isLogin = true;
  step = 1;
  phone = '';
  digits: string[] = ['', '', '', ''];
  loading = false;
  errorText = '';

  toggleMode(): void {
    this.isLogin = !this.isLogin;
    this.step = 1;
    this.errorText = '';
    this.phone = '';
    this.digits = ['', '', '', ''];
  }

  async sendOtp(): Promise<void> {
    if (this.phone.length < 9) return;
    this.loading = true;
    this.errorText = '';
    await this.delay(1000);
    this.loading = false;
    this.step = 2;
    setTimeout(() => {
      const first = document.querySelector<HTMLInputElement>('input[type="tel"][maxlength="1"]');
      first?.focus();
    }, 100);
  }

  async verifyOtp(): Promise<void> {
    const code = this.digits.join('');
    if (code.length < 4) return;
    this.loading = true;
    this.errorText = '';
    await this.delay(1000);
    this.loading = false;

    if (code === '1234') {
      this.router.navigate(['/patient-home']);
    } else {
      this.errorText = 'رمز التحقق غير صحيح. حاول مرة أخرى.';
      this.digits = ['', '', '', ''];
    }
  }

  resendOtp(): void {
    this.digits = ['', '', '', ''];
    this.errorText = '';
    // mock resend
  }

  onDigitInput(index: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    const val = input.value.replace(/\D/g, '');
    this.digits[index] = val ? val[0] : '';

    if (val && index < 3) {
      const inputs = document.querySelectorAll<HTMLInputElement>('input[maxlength="1"]');
      inputs[index + 1]?.focus();
    }

    // Auto verify when all filled
    if (this.digits.every((d) => d !== '')) {
      setTimeout(() => this.verifyOtp(), 200);
    }
  }

  onDigitKeydown(index: number, event: KeyboardEvent): void {
    if (event.key === 'Backspace' && !this.digits[index] && index > 0) {
      const inputs = document.querySelectorAll<HTMLInputElement>('input[maxlength="1"]');
      inputs[index - 1]?.focus();
    }
  }

  private delay(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
}
