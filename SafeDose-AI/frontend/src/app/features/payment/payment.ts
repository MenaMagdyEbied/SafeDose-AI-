import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  CircleCheck,
  CreditCard,
  Lock,
  LucideAngularModule,
  Shield,
  Smartphone,
} from 'lucide-angular';
import { OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-payment',
  imports: [LucideAngularModule, FormsModule],
  templateUrl: './payment.html',
  styleUrl: './payment.css',
})
export class Payment implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  creditCardIcon = CreditCard;
  smartphoneIcon = Smartphone;
  lockIcon = Lock;
  checkCircleIcon = CircleCheck;
  shieldIcon = Shield;

  method: 'card' | 'wallet' = 'card';
  showSuccess = false;

  card = { name: '', number: '', expiry: '', cvv: '' };
  wallet = { type: 'vodafone', phone: '' };

  wallets = [
    { id: 'vodafone', name: 'فودافون كاش', icon: '🔴' },
    { id: 'orange', name: 'اورنج كاش', icon: '🟠' },
    { id: 'etisalat', name: 'اتصالات كاش', icon: '🟢' },
  ];

  formatCardNumber(event: Event) {
    const input = event.target as HTMLInputElement;
    let val = input.value.replace(/\D/g, '').substring(0, 16);
    val = val.replace(/(.{4})/g, '$1 ').trim();
    this.card.number = val;
  }

  formatExpiry(event: Event) {
    const input = event.target as HTMLInputElement;
    let val = input.value.replace(/\D/g, '').substring(0, 4);
    if (val.length >= 2) val = val.substring(0, 2) + '/' + val.substring(2);
    this.card.expiry = val;
  }

  isValid(): boolean {
    if (this.method === 'card') {
      return (
        this.card.name.trim().length > 0 &&
        this.card.number.replace(/\s/g, '').length === 16 &&
        this.card.expiry.length === 5 &&
        this.card.cvv.length === 3
      );
    }
    return this.wallet.phone.length === 11 && this.wallet.phone.startsWith('01');
  }

  loading = false;
  planId = 'pro';

  plans: Record<string, { name: string; price: string; features: string[] }> = {
    pro: {
      name: 'SafeDose Pro ⭐',
      price: '٤٩',
      features: [
        'فحص تداخلات دوائية غير محدود',
        'مساعد ذكي متخصص',
        'تتبع أدوية متعددة',
        'تذكيرات ذكية',
      ],
    },
    family: {
      name: 'خطة العيلة 👨‍👩‍👧‍👦',
      price: '٩٩',
      features: [
        'كل مميزات Pro',
        'إضافة أفراد العيلة',
        'إدارة أدوية كل فرد',
        'تنبيهات لكل الأفراد',
      ],
    },
  };

  get planName() {
    return this.plans[this.planId]?.name ?? '';
  }
  get planPrice() {
    return this.plans[this.planId]?.price ?? '';
  }
  get planFeatures() {
    return this.plans[this.planId]?.features ?? [];
  }

  ngOnInit() {
    this.route.queryParams.subscribe((params: any) => {
      this.planId = params['plan'] ?? 'pro';
      if (params['success'] === 'true') {
        this.showSuccess = true;
      }
    });
  }

  pay() {
    this.loading = true;
    // TODO: POST /api/payment/create-order
    // البـ backend بيرجع Paymob payment URL
    // window.location.href = response.paymentUrl;

    // Simulate للـ demo
    setTimeout(() => {
      this.loading = false;
      this.showSuccess = true;
    }, 2000);
  }

  goHome() {
    this.showSuccess = false;
    this.router.navigate(['/home']);
  }
}
