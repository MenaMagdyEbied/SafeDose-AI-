import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Check, LucideAngularModule, Mail, ShieldCheck, TriangleAlert } from 'lucide-angular';
import { Auth } from '../../../core/auth/services/auth';

@Component({
  selector: 'app-email-confirmation',
  imports: [LucideAngularModule, FormsModule, RouterLink],
  templateUrl: './email-confirmation.html',
  styleUrl: './email-confirmation.css',
})
export class EmailConfirmation implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(Auth);

  shieldCheckIcon = ShieldCheck;
  alertTriangleIcon = TriangleAlert;

  email = '';
  digits: string[] = ['', '', '', ''];
  loading = false;
  errorText = '';

  ngOnInit(): void {
    this.email = this.route.snapshot.queryParams['email'] || '';
  }

  onDigitInput(index: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value.replace(/\D/g, '').slice(0, 1);
    this.digits[index] = value;
    if (value && index < this.digits.length - 1) {
      const next = document.querySelectorAll('input')[index + 1] as HTMLInputElement;
      next?.focus();
    }
  }

  onDigitKeydown(index: number, event: KeyboardEvent): void {
    if (event.key === 'Backspace' && !this.digits[index] && index > 0) {
      const prev = document.querySelectorAll('input')[index - 1] as HTMLInputElement;
      prev?.focus();
    }
  }

  confirm(): void {
    const code = this.digits.join('');
    if (code.length < 4) return;

    this.loading = true;
    this.errorText = '';

    this.authService.confirmEmail({ email: this.email, code }).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.loading = false;
        this.errorText = err?.error?.message || 'كود التأكيد غير صحيح. حاول مرة أخرى.';
      },
    });
  }
  ngOnDestroy(): void {
    throw new Error('Method not implemented.');
  }
}
