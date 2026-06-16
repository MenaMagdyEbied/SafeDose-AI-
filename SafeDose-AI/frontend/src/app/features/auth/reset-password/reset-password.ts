import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AlertTriangle, Check, Eye, EyeOff, Lock, LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-reset-password',
  imports: [LucideAngularModule, FormsModule, RouterLink],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css',
})
export class ResetPassword implements OnInit {
  private readonly route = inject(ActivatedRoute);

  lockIcon = Lock;
  alertIcon = AlertTriangle;
  checkIcon = Check;
  eyeIcon = Eye;
  eyeOffIcon = EyeOff;

  form = { email: '', code: '', newPassword: '', confirmPassword: '' };
  loading = false;
  resetDone = false;
  errorText = '';
  showPassword = false;
  showConfirm = false;

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.form.email = params['email'] ?? '';
    });
  }

  isValid(): boolean {
    return (
      this.form.email.trim().length > 0 &&
      this.form.code.trim().length > 0 &&
      this.form.newPassword.length >= 6 &&
      this.form.newPassword === this.form.confirmPassword
    );
  }

  async reset() {
    if (!this.isValid()) return;
    this.loading = true;
    this.errorText = '';

    try {
      const res = await fetch('https://localhost:54218/api/Auth/resetPassword', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          email: this.form.email,
          code: this.form.code,
          newPassword: this.form.newPassword,
          confirmPassword: this.form.confirmPassword,
        }),
      });

      if (res.ok) {
        this.resetDone = true;
      } else {
        const data = await res.json();
        this.errorText = data.message ?? 'الكود غير صحيح أو منتهي الصلاحية.';
      }
    } catch {
      this.errorText = 'حدث خطأ في الاتصال. تحقق من الإنترنت وحاول مرة أخرى.';
    } finally {
      this.loading = false;
    }
  }
}
