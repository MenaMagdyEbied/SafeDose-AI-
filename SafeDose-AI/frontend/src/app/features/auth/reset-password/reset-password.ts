import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
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

  loading = false;
  resetDone = false;
  errorText = '';
  showPassword = false;
  showConfirm = false;

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

  get email() {
    return this.form.get('email');
  }
  get code() {
    return this.form.get('code');
  }
  get newPassword() {
    return this.form.get('newPassword');
  }
  get confirmPassword() {
    return this.form.get('confirmPassword');
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

    this.loading = true;
    this.errorText = '';

    this.authService
      .resetPassword({
        email: this.email?.value,
        code: this.code?.value,
        newPassword: this.newPassword?.value,
        confirmPassword: this.confirmPassword?.value,
      })
      .subscribe({
        next: () => {
          this.loading = false;
          this.resetDone = true;
          this.router.navigate(['/login'], {
            queryParams: { email: this.email },
          });
        },
        error: (err) => {
          this.loading = false;
          this.errorText =
            (typeof err?.error === 'string' ? err.error : err?.error?.message) ||
            'الكود غير صحيح أو منتهي الصلاحية.';
        },
      });
  }
}
