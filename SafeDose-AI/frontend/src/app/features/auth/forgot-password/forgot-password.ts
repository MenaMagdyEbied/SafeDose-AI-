import { Component } from '@angular/core';
import { inject } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  NgForm,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
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

  email = '';
  loading = false;
  sent = false;
  errorText = '';
  touched = false;

  get isEmailInvalid(): boolean {
    const value = this.email.trim();
    if (!value) return true;
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return !emailRegex.test(value);
  }

  send(form: NgForm): void {
    this.touched = true;
    if (this.isEmailInvalid) return;

    this.loading = true;
    this.errorText = '';

    this.authService.forgotPassword({ email: this.email.trim() }).subscribe({
      next: () => {
        this.loading = false;
        this.sent = true;
      },
      error: (err) => {
        this.loading = false;
        this.errorText =
          (typeof err?.error === 'string' ? err.error : err?.error?.message) ||
          'البريد الإلكتروني غير مسجل.';
      },
    });
  }
}
