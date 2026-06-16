import { Component } from '@angular/core';
import {  inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TriangleAlert, Key, LucideAngularModule, Mail } from 'lucide-angular';

@Component({
  selector: 'app-forgot-password',
  imports: [LucideAngularModule, FormsModule, RouterLink],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css',
})
export class ForgotPassword {
  keyIcon = Key;
  mailIcon = Mail;
  alertIcon = TriangleAlert;

  email = '';
  loading = false;
  sent = false;
  errorText = '';

  async send() {
    if (!this.email.trim()) return;
    this.loading = true;
    this.errorText = '';

    try {
      const res = await fetch('https://localhost:54218/api/Auth/forgotPassword', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: this.email }),
      });

      if (res.ok) {
        this.sent = true;
      } else {
        const data = await res.json();
        this.errorText = data.message ?? 'البريد الإلكتروني غير مسجل.';
      }
    } catch {
      this.errorText = 'حدث خطأ في الاتصال. تحقق من الإنترنت وحاول مرة أخرى.';
    } finally {
      this.loading = false;
    }
  }
}
