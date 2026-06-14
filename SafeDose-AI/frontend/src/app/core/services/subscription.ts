import { Injectable } from '@angular/core';
import { AdminStats, SubscriptionPlan } from '../models';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class Subscription {
  private plansSubject = new BehaviorSubject<SubscriptionPlan[]>(this.getDefaultPlans());
  public plans$ = this.plansSubject.asObservable();

  private getDefaultPlans(): SubscriptionPlan[] {
    return [
      {
        id: 'free',
        nameAr: 'المجاني',
        nameEn: 'Free',
        price: 0,
        currency: 'EGP',
        features: [
          'إنشاء حساب مجاني',
          '٣ فحوصات تداخلات شهرياً',
          'تتبع دواء واحد',
          'مساعد ذكي أساسي',
          'بطاقة دواء رقمية',
        ],
        maxFamilyMembers: 0,
        maxInteractionChecks: 3,
        maxMedications: 1,
      },
      {
        id: 'family',
        nameAr: 'العائلي',
        nameEn: 'Family',
        price: 50,
        currency: 'EGP',
        features: [
          'حتى ٥ أفراد من العائلة',
          'فحوصات تداخلات غير محدودة',
          'تتبع أدوية غير محدود',
          'مساعد ذكي متقدم',
          'أولوية الدعم الطبي',
          'تقارير مفصلة للطبيب',
        ],
        maxFamilyMembers: 5,
        maxInteractionChecks: 999,
        maxMedications: 50,
      },
    ];
  }

  getPlans(): SubscriptionPlan[] {
    return this.plansSubject.value;
  }

  updatePlanPrice(planId: 'free' | 'family', newPrice: number): void {
    const plans = this.plansSubject.value.map((p) =>
      p.id === planId ? { ...p, price: newPrice } : p,
    );
    this.plansSubject.next(plans);
  }
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private mockStats: AdminStats = {
    totalUsers: 12847,
    paidSubscriptions: 3421,
    totalProfit: 171050,
    totalDashboards: 8934,
    recentSignups: 156,
    activeUsers: 6789,
  };

  getStats(): AdminStats {
    return { ...this.mockStats };
  }
}
