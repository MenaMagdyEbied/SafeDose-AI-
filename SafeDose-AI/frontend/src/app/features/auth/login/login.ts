import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ArrowLeft, Eye, EyeOff, LucideAngularModule, ShieldCheck, TriangleAlert } from 'lucide-angular';
import { Auth } from '../../../core/auth/services/auth';

@Component({
  selector: 'app-login',
  imports: [FormsModule, LucideAngularModule, RouterLink,ReactiveFormsModule],

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

  showPassword = false;
  loading = false;
  errorText = '';

  loginForm: FormGroup = this.fb.group({
    userName: ['', [Validators.required, Validators.minLength(3)]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  get userName() {
    return this.loginForm.get('userName');
  }
  get password() {
    return this.loginForm.get('password');
  }

  submit(): void {
    this.loginForm.markAllAsTouched();
    if (this.loginForm.invalid) return;

    this.loading = true;
    this.errorText = '';

    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.loading = false;
        if (this.authService.isAdmin) {
          this.router.navigate(['/admin']);
        } else {
          this.router.navigate(['/home']);
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorText = err?.error?.message || 'بيانات الدخول غير صحيحة.';
      },
    });
  }
}
