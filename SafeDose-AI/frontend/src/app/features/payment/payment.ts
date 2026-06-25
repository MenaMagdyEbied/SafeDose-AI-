import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CircleCheck, Lock, LucideAngularModule, Shield } from 'lucide-angular';
import { EMPTY, from } from 'rxjs';
import { catchError, finalize, switchMap } from 'rxjs/operators';
import { Billing } from '../../core/services/billing';
import { Auth } from '../../core/auth/services/auth';

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
  private readonly destroyRef = inject(DestroyRef);

  lockIcon = Lock;
  checkCircleIcon = CircleCheck;
  shieldIcon = Shield;

  loading = false;
  showSuccess = false;
  showFailure = false;
  showError = false;
  errorText = '';
  planId = 'premium-monthly';
  verifying = false;
  method: 'card' | 'wallet' = 'card';
  subscribedPlanName = '';

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

    this.route.queryParams
      .pipe(
        switchMap((params: any) => {
          this.planId = params['plan'] ?? 'premium-monthly';
          const merchantOrderId = params['merchant_order_id'];
          const paymobSuccess =
            params['success'] === undefined ? undefined : params['success'] === 'true';

          if (!merchantOrderId) {
            return EMPTY;
          }

          this.verifying = true;
          return from(this.billingService.getPaymentStatus(merchantOrderId, paymobSuccess)).pipe(
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
      .subscribe((data: any) => {
        if (data?.success || data?.subscriptionActive) {
          this.subscribedPlanName = data.tierName || this.planName;
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
        const paymentUrl = data.paymentUrl || data.iframeUrl;
        if (!paymentUrl) {
          this.errorText = 'لم يتم إنشاء رابط الدفع من Paymob.';
          this.showError = true;
          this.showFailure = true;
          return;
        }

        window.location.href = paymentUrl;
      });
  }
  verifyPayment(merchantOrderId: string): void {
    from(this.billingService.getPaymentStatus(merchantOrderId))
      .pipe(
        catchError(() => {
          this.errorText = 'فشل التحقق من حالة الدفع.';
          this.showError = true;
          this.showFailure = true;
          return EMPTY;
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((data) => {
        if (data.success || data.subscriptionActive) {
          this.subscribedPlanName = data.tierName || this.planName;
          this.showSuccess = true;
          this.showFailure = false;
        } else {
          this.errorText = 'لم يتم تأكيد الدفع. إذا تم خصم المبلغ، تواصل مع الدعم.';
          this.showError = true;
          this.showFailure = true;
        }
      });
  }

  closeFailureModal(): void {
    this.showFailure = false;
  }

  goHome(): void {
    this.showSuccess = false;
    this.router.navigate(['/home']);
  }
}
