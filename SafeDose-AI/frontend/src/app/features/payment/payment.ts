import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CircleCheck, Lock, LucideAngularModule, Shield } from 'lucide-angular';
import { Billing } from '../../core/services/billing';

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

  lockIcon = Lock;
  checkCircleIcon = CircleCheck;
  shieldIcon = Shield;

  loading = false;
  showSuccess = false;
  showError = false;
  errorText = '';
  planId = 'pro';
  verifying = false;

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

  ngOnInit(): void {
    this.route.queryParams.subscribe(async (params: any) => {
      this.planId = params['plan'] ?? 'pro';

      const merchantOrderId = params['merchant_order_id'];
      if (merchantOrderId) {
        this.verifying = true;
        await this.verifyPayment(merchantOrderId);
        this.verifying = false;
      }
    });
  }

  async pay(): Promise<void> {
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

    try {
      const data = await this.billingService.checkout({
        tierCode: this.tierCode,
        paymentMethod: 'paymob',
        fullName: this.userForm.fullName,
        email: this.userForm.email,
        phoneNumber: this.userForm.phoneNumber,
      });

      window.location.href = data.paymentUrl;
    } catch (err: any) {
      this.errorText =
        (typeof err?.error === 'string' ? err.error : err?.error?.message) ||
        'حدث خطأ أثناء إنشاء طلب الدفع.';
      this.showError = true;
    } finally {
      this.loading = false;
    }
  }

  async verifyPayment(merchantOrderId: string): Promise<void> {
    try {
      const data = await this.billingService.getPaymentStatus(merchantOrderId);
      if (data.success) {
        this.showSuccess = true;
      } else {
        this.errorText = 'لم يتم تأكيد الدفع. إذا تم خصم المبلغ، تواصل مع الدعم.';
        this.showError = true;
      }
    } catch {
      this.errorText = 'فشل التحقق من حالة الدفع.';
      this.showError = true;
    }
  }

  goHome(): void {
    this.showSuccess = false;
    this.router.navigate(['/home']);
  }
}

