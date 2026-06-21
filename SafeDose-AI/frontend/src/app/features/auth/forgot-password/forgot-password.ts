import { Component, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TriangleAlert, Key, LucideAngularModule, Mail } from 'lucide-angular';
import { Auth } from '../../../core/auth/services/auth';

@Component({
  selector: 'app-forgot-password',
  imports: [LucideAngularModule, FormsModule, RouterLink],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css',
})
export class ForgotPassword {
  private readonly authService = inject(Auth);
  private readonly router = inject(Router);

  keyIcon = Key;
  mailIcon = Mail;
  alertIcon = TriangleAlert;

  email = signal('');
  loading = signal(false);
  sent = signal(false);
  errorText = signal('');
  touched = signal(false);

  get isEmailInvalid(): boolean {
    const value = this.email().trim();
    if (!value) return true;
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return !emailRegex.test(value);
  }

  send(form: NgForm): void {
    this.touched.set(true);
    if (this.isEmailInvalid) return;

    this.loading.set(true);
    this.errorText.set('');

    this.authService.forgotPassword({ email: this.email().trim() }).subscribe({
      next: () => {
        this.loading.set(false);
        this.sent.set(true);
        this.router.navigate(['/reset-password'], {
          queryParams: { email: this.email() },
        });
      },
      error: (err) => {
        this.loading.set(false);
        this.errorText.set(
          (typeof err?.error === 'string' ? err.error : err?.error?.message) ||
            'البريد الإلكتروني غير مسجل.',
        );
      },
    });
  }
}
