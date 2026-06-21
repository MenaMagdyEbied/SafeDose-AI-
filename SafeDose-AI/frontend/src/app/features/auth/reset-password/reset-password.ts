import { Component, OnInit, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Check, Eye, EyeOff, Lock, LucideAngularModule, TriangleAlert } from 'lucide-angular';
import { Auth } from '../../../core/auth/services/auth';
import { passwordsMatchValidator } from '../../../shared/validators/passwords-match-validator';

@Component({
  selector: 'app-reset-password',
  imports: [LucideAngularModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css',
})
export class ResetPassword implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly authService = inject(Auth);
  private readonly router = inject(Router);

  lockIcon = Lock;
  alertIcon = TriangleAlert;
  checkIcon = Check;
  eyeIcon = Eye;
  eyeOffIcon = EyeOff;

  loading = signal(false);
  resetDone = signal(false);
  errorText = signal('');
  showPassword = signal(false);
  showConfirm = signal(false);

  form: FormGroup = this.fb.group(
    {
      email: ['', [Validators.required, Validators.email]],
      code: ['', [Validators.required, Validators.pattern(/^[0-9]{6}$/)]],
      newPassword: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          Validators.pattern(/^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$_%^&*-]).{8,}$/),
        ],
      ],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatchValidator },
  );

  get email(): AbstractControl {
    return this.form.get('email')!;
  }
  get code(): AbstractControl {
    return this.form.get('code')!;
  }
  get newPassword(): AbstractControl {
    return this.form.get('newPassword')!;
  }
  get confirmPassword(): AbstractControl {
    return this.form.get('confirmPassword')!;
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      if (params['email']) {
        this.form.patchValue({ email: params['email'] });
      }
    });
  }

  reset(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.loading.set(true);
    this.errorText.set('');

    this.authService
      .resetPassword({
        email: this.email?.value,
        code: this.code?.value,
        newPassword: this.newPassword?.value,
        confirmPassword: this.confirmPassword?.value,
      })
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.resetDone.set(true);
          this.router.navigate(['/login'], {
            queryParams: { email: this.email?.value },
          });
        },
        error: (err) => {
          this.loading.set(false);
          this.errorText.set(
            (typeof err?.error === 'string' ? err.error : err?.error?.message) ||
              'الكود غير صحيح أو منتهي الصلاحية.',
          );
        },
      });
  }
}
