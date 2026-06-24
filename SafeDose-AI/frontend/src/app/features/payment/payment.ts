import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CircleCheck, Lock, LucideAngularModule, Shield } from 'lucide-angular';
import { EMPTY, from } from 'rxjs';
import { catchError, finalize, switchMap } from 'rxjs/operators';
import { Billing } from '../../core/services/billing';
import { Auth } from '../../core/auth/services/auth';
import { Subscription } from '../../core/services/subscription';

@Component({
  selector: 'app-payment',
  imports: [LucideAngularModule, FormsModule],
  templateUrl: './payment.html',
  styleUrl: './payment.css',
})
export class Payment implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly billingService = inject(Billing);
  private readonly authService = inject(Auth);
  private readonly subscriptionService = inject(Subscription);
  private readonly destroyRef = inject(DestroyRef);

  lockIcon = Lock;
  checkCircleIcon = CircleCheck;
  shieldIcon = Shield;

  loading = false;
  showSuccess = false;
  showFailure = false;
  showError = false;
  errorText = '';
  planId = 'pro';
  verifying = false;
  method: 'card' | 'wallet' = 'card';

  userForm = {
    fullName: '',
    email: '',
    phoneNumber: '',
  };

  plans: Record<string, { name: string; price: string; features: string[] }> = {
    'premium-monthly': {
      name: 'بريميوم شهري ⭐',
      price: '٣٠',
      features: [
        'فحص تداخلات دوائية غير محدود',
        'مساعد ذكي متخصص',
        'تتبع حتى ٥ مرضى',
        'تذكيرات ذكية',
      ],
    },
    'premium-annual': {
      name: 'بريميوم سنوي 👑',
      price: '٣٠٠',
      features: ['كل مميزات البريميوم الشهري', 'توفير ٦٠ جنيه سنوياً', 'أولوية في الدعم'],
    },
  };

  get tierCode() {
    return this.plans[this.planId] ? this.planId : 'premium-monthly';
  }
  get planName() {
    return this.plans[this.planId]?.name ?? '';
  }
  get planPrice() {
    return this.plans[this.planId]?.price ?? '';
  }
  get planFeatures() {
    return this.plans[this.planId]?.features ?? [];
  }
  get paymentMethodValue(): string {
    return this.method === 'card' ? 'card' : 'wallet';
  }

  ngOnInit(): void {
    this.prefillUserData();

    if (this.authService.isLoggedIn) {
      this.subscriptionService.refresh().then((sub) => {
        if (sub?.isActive && sub.tierCode !== 'free') {
          this.router.navigate(['/profile']);
        }
      });
    }

    this.route.queryParams
      .pipe(
        switchMap((params: any) => {
          this.planId = params['plan'] ?? 'pro';
          const merchantOrderId = params['merchant_order_id'];
          const paymobSuccess = String(params['success']).toLowerCase() === 'true';

          if (!merchantOrderId) {
            return EMPTY;
          }

          this.verifying = true;
          const statusPromise = paymobSuccess
            ? this.billingService.waitForPaymentConfirmation(merchantOrderId)
            : this.billingService.getPaymentStatus(merchantOrderId);

          return from(statusPromise).pipe(
            catchError(() => {
              this.errorText = 'فشل التحقق من حالة الدفع.';
              this.showError = true;
              this.showFailure = true;
              return EMPTY;
            }),
            finalize(() => {
              this.verifying = false;
            }),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(async (data: any) => {
        if (data?.success || data?.subscriptionActive) {
          await this.subscriptionService.refresh();
          this.showSuccess = true;
          this.showFailure = false;
        } else if (data) {
          this.errorText = 'لم يتم تأكيد الدفع. إذا تم خصم المبلغ، تواصل مع الدعم.';
          this.showError = true;
          this.showFailure = true;
        }
      });
  }

  private prefillUserData(): void {
    const currentUser = this.authService.user;
    if (currentUser) {
      this.userForm.fullName = currentUser.name || currentUser.userName || '';
      this.userForm.email = currentUser.email || '';
      this.userForm.phoneNumber = currentUser.phone || '';
    }
  }

  pay(): void {
    if (
      !this.userForm.fullName.trim() ||
      !this.userForm.email.trim() ||
      !this.userForm.phoneNumber.trim()
    ) {
      this.errorText = 'يرجى ملء جميع البيانات';
      this.showError = true;
      return;
    }

    if (this.method === 'wallet' && this.userForm.phoneNumber.replace(/\D/g, '').length < 10) {
      this.errorText = 'أدخل رقم فودافون كاش صحيح (مثال: 01012345678)';
      this.showError = true;
      return;
    }

    this.loading = true;
    this.showError = false;
    this.showFailure = false;

    from(
      this.billingService.checkout({
        tierCode: this.tierCode,
        paymentMethod: this.paymentMethodValue,
        fullName: this.userForm.fullName,
        email: this.userForm.email,
        phoneNumber: this.userForm.phoneNumber,
      }),
    )
      .pipe(
        catchError((err: any) => {
          this.errorText =
            (typeof err?.error === 'string' ? err.error : err?.error?.message) ||
            'حدث خطأ أثناء إنشاء طلب الدفع.';
          this.showError = true;
          this.showFailure = true;
          return EMPTY;
        }),
        finalize(() => {
          this.loading = false;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((data) => {
        const url = data.paymentUrl ?? data.iframeUrl;
        if (!url) {
          this.errorText = 'لم يُرجع الخادم رابط الدفع. حاول مرة أخرى.';
          this.showError = true;
          this.showFailure = true;
          return;
        }
        window.location.href = url;
      });
  }

  closeFailureModal(): void {
    this.showFailure = false;
  }

  async goHome(): Promise<void> {
    this.showSuccess = false;
    await this.subscriptionService.refresh();
    this.router.navigate(['/profile']);
  }
}
