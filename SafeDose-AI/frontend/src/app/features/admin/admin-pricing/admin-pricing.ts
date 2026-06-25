import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  LucideAngularModule,
  Crown,
  Shield,
  Trash2,
  Plus,
  Save,
  Info,
  CheckCircle,
  CircleCheck,
} from 'lucide-angular';
import { Plan } from '../../../core/models/plan';
import { PricingTier } from '../../../core/models/pricing-tier';
import { Subscription } from '../../../core/services/subscription';

@Component({
  selector: 'app-admin-pricing',
  imports: [LucideAngularModule, FormsModule],
  templateUrl: './admin-pricing.html',
  styleUrl: './admin-pricing.css',
})
export class AdminPricing implements OnInit {
  private readonly subscriptionService = inject(Subscription);

  crownIcon = Crown;
  shieldIcon = Shield;
  trashIcon = Trash2;
  plusIcon = Plus;
  saveIcon = Save;
  infoIcon = Info;
  checkCircleIcon = CircleCheck;

  savedMessage = '';

  plans: Plan[] = this.getDefaultPlans();

  ngOnInit(): void {
    this.loadPlans();
  }

  addFeature(plan: Plan): void {
    plan.features.push('');
  }

  removeFeature(plan: Plan, index: number): void {
    plan.features.splice(index, 1);
  }

  async saveAll(): Promise<void> {
    const tiers = this.mapPlansToTiers(this.plans);
    await this.subscriptionService.saveTiers(tiers);
    this.savedMessage = 'تم حفظ التغييرات بنجاح ✅';
    setTimeout(() => (this.savedMessage = ''), 3000);
  }

  private async loadPlans(): Promise<void> {
    const tiers = await this.subscriptionService.getTiers();
    this.plans = this.mapTiersToPlans(tiers);
  }

  private mapTiersToPlans(tiers: PricingTier[]): Plan[] {
    const defaults = this.getDefaultPlans();
    return defaults.map((defaultPlan) => {
      const tier = tiers.find((item) => item.tierCode === defaultPlan.id);
      return {
        id: defaultPlan.id,
        nameAr: tier?.nameAr || tier?.tierName || defaultPlan.nameAr,
        nameEn: tier?.nameEn || defaultPlan.nameEn,
        price: tier?.price ?? defaultPlan.price,
        features: [...(tier?.features ?? defaultPlan.features)],
      };
    });
  }

  private mapPlansToTiers(plans: Plan[]): PricingTier[] {
    return plans.map((plan, index) => {
      const priceLabelArabic =
        plan.price === 0
          ? 'مجاني'
          : plan.id === 'premium-annual'
            ? `${plan.price} ج.م / سنة`
            : `${plan.price} ج.م / شهر`;

      return {
        pricingTierId: index + 1,
        tierCode: plan.id,
        tierName: plan.nameAr,
        price: plan.price,
        currency: 'EGP',
        patientLimit: plan.id === 'free' ? 1 : 5,
        priceLabelArabic,
        features: [...plan.features],
        nameAr: plan.nameAr,
        nameEn: plan.nameEn,
      };
    });
  }

  private getDefaultPlans(): Plan[] {
    return [
      {
        id: 'free',
        nameAr: 'المجاني',
        nameEn: 'Free',
        price: 0,
        features: [
          'إنشاء حساب',
          'فحص ٣ تداخلات دوائية شهرياً',
          'تتبع دواء واحد',
          'مساعد ذكي أساسي',
        ],
      },
      {
        id: 'premium-monthly',
        nameAr: 'مدفوع شهرياً',
        nameEn: 'Premium Monthly',
        price: 99,
        features: [
          'كل ميزات الباقة المجانية',
          'فحص تداخلات دوائية غير محدود',
          'تتبع أدوية غير محدود',
          'حتى ٥ أفراد عائلة',
          'مساعد ذكي متقدم',
          'تقارير للطبيب',
          'أولوية الدعم',
        ],
      },
      {
        id: 'premium-annual',
        nameAr: 'مدفوع سنوياً',
        nameEn: 'Premium Annual',
        price: 990,
        features: [
          'كل ميزات الباقة الشهرية',
          'توفير ١١٨ ج.م عن الاشتراك الشهري',
          'أولوية في الدعم الفني',
          'تحديثات مجانية للسنة',
        ],
      },
    ];
  }
}
