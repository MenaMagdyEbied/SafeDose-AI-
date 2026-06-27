import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LucideAngularModule, ShieldCheck, TriangleAlert } from 'lucide-angular';
import { Auth } from '../../../core/auth/services/auth';

@Component({
  selector: 'app-email-confirmation',
  imports: [LucideAngularModule, FormsModule, RouterLink],
  templateUrl: './email-confirmation.html',
  styleUrl: './email-confirmation.css',
})
export class EmailConfirmation implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(Auth);

  shieldCheckIcon = ShieldCheck;
  alertTriangleIcon = TriangleAlert;

  email = '';
  digits: string[] = ['', '', '', '', '', ''];
  codeTouched = false;
  loading = signal(false);
  resendLoading = signal(false);
  errorText = '';
  successText = '';

  ngOnInit(): void {
    this.email = this.route.snapshot.queryParams['email'] || '';
  }

  get isCodeComplete(): boolean {
    return this.digits.every((d) => d !== '');
  }

  onDigitInput(index: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value.replace(/\D/g, '').slice(0, 1);
    this.digits[index] = value;

    if (value && index < this.digits.length - 1) {
      const next = document.querySelectorAll('input')[index + 1] as HTMLInputElement;
      next?.focus();
    }

    if (this.isCodeComplete) {
      this.confirm();
    }
  }

  onDigitKeydown(index: number, event: KeyboardEvent): void {
    if (event.key === 'Backspace' && !this.digits[index] && index > 0) {
      const prev = document.querySelectorAll('input')[index - 1] as HTMLInputElement;
      prev?.focus();
    }
  }

  onDigitBlur(): void {
    this.codeTouched = true;
  }

  confirm(): void {
    this.codeTouched = true;
    if (!this.isCodeComplete) return;

    const code = this.digits.join('');
    this.loading.set(true);
    this.errorText = '';
    this.successText = '';

    this.authService.confirmEmail({ email: this.email, code }).subscribe({
      next: () => {
        this.loading.set(false);
        try {
          localStorage.removeItem('safedose_registration_draft');
        } catch {
          // Ignore storage failures in restricted environments.
        }
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorText =
          (typeof err?.error === 'string' ? err.error : err?.error?.message) ||
          'كود التأكيد غير صحيح. حاول مرة أخرى.';
        this.digits = ['', '', '', '', '', ''];
        // this.router.navigate(['/email-confirmation']);
      },
    });
  }

  resendCode(): void {
    if (!this.email) return;

    this.resendLoading.set(true);
    this.errorText = '';
    this.successText = '';

    this.authService.resendCode(this.email).subscribe({
      next: () => {
        this.resendLoading.set(false);
        this.successText = 'تم إعادة إرسال كود التأكيد بنجاح.';
      },
      error: (err) => {
        this.resendLoading.set(false);
        this.errorText =
          (typeof err?.error === 'string' ? err.error : err?.error?.message) ||
          'تعذر إعادة إرسال كود التأكيد. حاول مرة أخرى.';
      },
    });
  }
}
