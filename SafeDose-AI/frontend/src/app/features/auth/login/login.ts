import { Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  ArrowLeft,
  Eye,
  EyeOff,
  LucideAngularModule,
  ShieldCheck,
  TriangleAlert,
} from 'lucide-angular';
import { Auth } from '../../../core/auth/services/auth';

@Component({
  selector: 'app-login',
  imports: [FormsModule, LucideAngularModule, RouterLink, ReactiveFormsModule],

  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(Auth);
  private readonly router = inject(Router);

  arrowLeftIcon = ArrowLeft;
  shieldCheckIcon = ShieldCheck;
  alertTriangleIcon = TriangleAlert;
  eyeIcon = Eye;
  eyeOffIcon = EyeOff;

  showPassword = signal(false);
  loading = signal(false);
  errorText = signal('');

  loginForm: FormGroup = this.fb.group({
    userName: ['', [Validators.required, Validators.minLength(3)]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  get userName(): AbstractControl {
    return this.loginForm.get('userName')!;
  }
  get password(): AbstractControl {
    return this.loginForm.get('password')!;
  }

  submit(): void {
    this.loginForm.markAllAsTouched();
    if (this.loginForm.invalid) return;

    this.loading.set(true);
    this.errorText.set('');

    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.loading.set(false);
        if (this.authService.isAdmin) {
          this.router.navigate(['/admin']);
        } else {
          this.router.navigate(['/home']);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.errorText.set(err?.error?.message || 'بيانات الدخول غير صحيحة.');
      },
    });
  }
}
