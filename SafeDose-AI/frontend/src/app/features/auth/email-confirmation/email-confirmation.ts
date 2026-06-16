import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  Check,
  LucideAngularModule,
  Mail,
  ShieldCheck,
  TriangleAlert
} from 'lucide-angular';

@Component({
  selector: 'app-email-confirmation',
  imports: [LucideAngularModule, FormsModule, RouterLink],
  templateUrl: './email-confirmation.html',
  styleUrl: './email-confirmation.css',
})
export class EmailConfirmation implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);

  mailIcon = Mail;
  alertIcon = TriangleAlert;
  checkIcon = Check;
  shieldCheckIcon = ShieldCheck;

  email = '';
  code = '';
  loading = false;
  confirmed = false;
  errorText = '';
  successText = '';
  resendCooldown = 0;
  private cooldownInterval: any;

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.email = params['email'] ?? '';
    });
  }

  async confirm() {
    if (this.code.length < 6) return;
    this.loading = true;
    this.errorText = '';

    try {
      const res = await fetch('https://localhost:54218/api/Auth/emailConfirmation', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: this.email, code: this.code }),
      });

      if (res.ok) {
        this.confirmed = true;
      } else {
        const data = await res.json();
        this.errorText = data.message ?? 'كود التأكيد غير صحيح. حاول مرة أخرى.';
      }
    } catch {
      this.errorText = 'حدث خطأ في الاتصال. تحقق من الإنترنت وحاول مرة أخرى.';
    } finally {
      this.loading = false;
    }
  }

  async resend() {
    this.successText = '';
    this.errorText = '';

    try {
      // TODO: endpoint إعادة الإرسال لو موجود
      this.successText = 'تم إرسال كود جديد إلى بريدك الإلكتروني';
      this.startCooldown();
    } catch {
      this.errorText = 'فشل إعادة الإرسال. حاول مرة أخرى.';
    }
  }

  startCooldown() {
    this.resendCooldown = 60;
    this.cooldownInterval = setInterval(() => {
      this.resendCooldown--;
      if (this.resendCooldown <= 0) clearInterval(this.cooldownInterval);
    }, 1000);
  }

  ngOnDestroy() {
    clearInterval(this.cooldownInterval);
  }
}
